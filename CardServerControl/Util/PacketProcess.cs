﻿using CardServerControl.Model;
using CardServerControl.Model.DTO;
using System;
using System.Data;
using System.Net;
using System.Collections.Generic;
using LitJson;

namespace CardServerControl.Util
{
    class PacketProcess
    {
        /// <summary>
        /// 检查UUID的合法性
        /// </summary>
        private bool CheckUUID(string uuid)
        {
            string command = string.Format("SELECT * FROM account WHERE UUID = '{0}'", uuid);
            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
            if (ds.Tables[0].Rows.Count > 0)//UUID验证通过
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 处理登陆包
        /// </summary>
        /// <param name="data">登陆数据</param>
        /// <param name="iped">发送的ip</param>
        /// <returns>返回封包</returns>
        public SocketModel LoginPacket(LoginDTO data, IPEndPoint iped)
        {
            SocketModel model = new SocketModel();
            model.areaCode = AreaCode.Server;
            model.protocol = SocketProtocol.LOGIN;

            string account = data.account;
            string password = data.password;

            if (PlayerManager.Instance.CanLogin())
            {
                //查询数据库
                string command = string.Format("SELECT * FROM account WHERE Account = '{0}' AND Password = '{1}'", account, password);
                DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

                if (ds.Tables[0].Rows.Count == 1)
                {
                    //为数据表创建uuid并写入
                    string uuid = System.Guid.NewGuid().ToString();
                    command = string.Format("UPDATE account SET UUID = '{0}',LastLogin = '{1}' WHERE Account = '{2}' AND Password = '{3}'", uuid, CommonDTO.GetTimeStamp().ToString(), account, password);
                    MySQLHelper.ExecuteNonQuery(MySQLHelper.Conn, CommandType.Text, command, null);

                    //获取该用户的uid和玩家名
                    int uid = Convert.ToInt32(ds.Tables[0].Rows[0]["Uid"]);
                    command = string.Format("SELECT PlayerName FROM playerinfo WHERE Uid = '{0}'", uid);
                    ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                    string playerName = ds.Tables[0].Rows[0]["PlayerName"].ToString();

                    //添加到服务器的用户列表
                    PlayerManager.Instance.PlayerLoginLobby(uid, playerName, uuid, iped);

                    //构造返回数据
                    if (data.internalVersion == MainWindow.instance.internalVersion)
                    {
                        //内部版本号一致
                        model.returnCode = ReturnCode.Success;
                    }
                    else
                    {
                        if (data.officialVersion == MainWindow.instance.officialVersion)
                        {
                            //外部版本号一致
                            model.returnCode = ReturnCode.Pass;
                        }
                        else
                        {
                            //外部版本号不一致
                            model.returnCode = ReturnCode.Repeal;
                        }
                    }

                    
                    LoginDTO returnData = new LoginDTO();
                    returnData.account = data.account;
                    returnData.password = data.password;
                    returnData.playerName = playerName;
                    returnData.UUID = uuid;
                    model.message = JsonCoding<LoginDTO>.encode(returnData);
                }
                else
                {
                    //登陆失败
                    LogsSystem.Instance.Print(string.Format("账户{0}[{1}]试图登陆游戏失败：用户名或密码错误", account, iped.Address.ToString()));
                    model.message = JsonCoding<LoginDTO>.encode(data);
                    model.returnCode = ReturnCode.Failed;
                }
            }
            else
            {
                //服务器已满
                LogsSystem.Instance.Print(string.Format("账户{0}[{1}]试图登陆游戏失败：服务器已满", account, iped.Address.ToString()));
                model.message = JsonCoding<LoginDTO>.encode(data);
                model.returnCode = ReturnCode.Refuse;
            }

            return model;
        }

        /// <summary>
        /// 处理聊天包
        /// </summary>
        /// <param name="data">聊天数据</param>
        /// <returns>返回封包</returns>
        public SocketModel ChatPacket(ChatDTO data)
        {
            string content = data.content;
            string senderName = data.senderName;
            string senderUUID = data.senderUUID;
            string toUUID = data.toUUID;

            //检查UUID
            if (!CheckUUID(senderUUID))
            {
                return Offline();
            }

            //群发
            foreach (Player player in PlayerManager.Instance.GetLobbyPlayerList())
            {
                if (player.UUID != senderUUID)
                {
                    SocketModel chatmodel = new SocketModel();
                    chatmodel.areaCode = AreaCode.Server;
                    chatmodel.protocol = SocketProtocol.CHAT;
                    chatmodel.message = JsonCoding<ChatDTO>.encode(new ChatDTO(content, senderName, senderUUID));

                    UdpServer.Instance.SendToPlayerByUUID(JsonCoding<SocketModel>.encode(chatmodel), player.UUID);
                }
            }

            return null;//不返回数据包
        }

        /// <summary>
        /// 处理玩家信息包
        /// </summary>
        public SocketModel PlayerInfoPacket(PlayerInfoDTO data)
        {
            string UUID = data.UUID;

            //检查UUID
            if (!CheckUUID(UUID))
            {
                return Offline();
            }

            Player senderPlayer = PlayerManager.Instance.GetLobbyPlayerByUUID(UUID);
            int uid = senderPlayer.uid;

            string command = string.Format("SELECT * FROM playerinfo WHERE Uid = '{0}'", uid);
            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

            //构建返回数据包
            SocketModel model = new SocketModel();
            model.areaCode = AreaCode.Server;
            model.protocol = SocketProtocol.PLAYERINFO;
            model.returnCode = ReturnCode.Success;

            PlayerInfoDTO returnData = new PlayerInfoDTO();
            returnData.UUID = UUID;
            returnData.uid = uid;
            returnData.playerName = ds.Tables[0].Rows[0]["PlayerName"].ToString();
            returnData.level = Convert.ToInt32(ds.Tables[0].Rows[0]["Level"]);
            returnData.coin = Convert.ToInt32(ds.Tables[0].Rows[0]["Coin"]);
            returnData.gem = Convert.ToInt32(ds.Tables[0].Rows[0]["Gem"]);
            

            //returnData.vipExpire = DateTime.Parse(ds.Tables[0].Rows[0]["VipExpire"].ToString());

            model.message = JsonCoding<PlayerInfoDTO>.encode(returnData);

            return model;
        }

        /// <summary>
        /// 处理卡片信息包
        /// </summary>
        public SocketModel CardInfoPacket(CardInfoDTO data)
        {
            CardInfoDTO returnData = new CardInfoDTO();
            List<CardInfo> cardInfoList = new List<CardInfo>();
            int cardOwnerId = data.cardOwnerId;

            //从数据库获取信息
            string command = string.Format("SELECT * FROM cardinventory WHERE CardOwnerId = '{0}'", cardOwnerId);
            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                CardInfo cardInfo = new CardInfo();
                int cardId = Convert.ToInt32(row["CardId"]);
                cardInfo.cardId = cardId;
                cardInfo.cardOwnerId = Convert.ToInt32(row["CardOwnerId"]);
                cardInfo.cardRarity = UdpServer.Instance.cardManager.GetRarityByCardId(cardId);
                cardInfo.cardName = UdpServer.Instance.cardManager.GetNameById(cardId);
                cardInfo.cardLevel = Convert.ToInt32(row["CardLevel"]);

                cardInfoList.Add(cardInfo);
            }
            returnData.cardOwnerId = cardOwnerId;
            returnData.cardInfoList = cardInfoList.ToArray();

            SocketModel model = new SocketModel();
            model.returnCode = ReturnCode.Success;
            model.protocol = SocketProtocol.CARDINFOLIST;
            model.message = JsonCoding<CardInfoDTO>.encode(returnData);

            return model;
        }

        public SocketModel InvInfoPacket(InvInfoDTO data)
        {
            int playerID = data.playerID;
            string playerUUID = data.playerUUID;
            int type = data.type;//player = 1,Hero = 2,Guide = 3,Inv = 4

            int returnCode = ReturnCode.Success;//设定返回代码
            if (CheckUUID(playerUUID))
            {
                string returnData = "";
                switch (type)
                {
                    case 1://玩家页
                        {
                            JsonData json = new JsonData();
                            json.SetJsonType(JsonType.Array);//设置为数组

                            //从数据库中获取玩家所有的卡片的ID、类型、是否上场并添加到返回字符串
                            string command = string.Format("SELECT CardId,CardType,IsUsing FROM cardinventory WHERE CardOwnerId = '{0}'", playerID);
                            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                JsonData rowJson = new JsonData();
                                rowJson["CardId"] = Convert.ToInt32(row["CardId"]);
                                rowJson["CardType"] = row["CardType"].ToString();
                                rowJson["IsUsing"] = Convert.ToBoolean(row["IsUsing"]);

                                json.Add(rowJson);
                            }

                            returnData = json.ToJson();

                            break;
                        }
                    case 2://英雄页
                        {
                            JsonData json = new JsonData();
                            json.SetJsonType(JsonType.Array);//设置为数组

                            //从数据库中获取玩家所有的英雄卡片的ID、是否上场并添加到返回字符串
                            string command = string.Format("SELECT CardId,IsUsing FROM cardinventory WHERE CardOwnerId = '{0}' AND CardType = '{1}'", playerID,"Character");
                            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                JsonData rowJson = new JsonData();
                                rowJson["CardId"] = Convert.ToInt32(row["CardId"]);
                                rowJson["IsUsing"] = Convert.ToBoolean(row["IsUsing"]);

                                json.Add(rowJson);
                            }

                            returnData = json.ToJson();

                            break;
                        }
                    case 3://图鉴页
                        {
                            JsonData json = new JsonData();
                            json.SetJsonType(JsonType.Array);//设置为数组

                            //从数据库中获取玩家所有的英雄卡片的ID、是否上场并添加到返回字符串
                            string format = "SELECT *,COUNT(inv.CardId) AS OwnNum, " +
                                "MAX(inv.SpecialAttack + inv.SpecialEnergy+ inv.SpecialHealth+ inv.SpecialSpeed) AS Talent " +
                                "FROM cardinventory as inv " +
                                "LEFT JOIN card " +
                                "ON inv.CardId = card.CardId " +
                                "WHERE inv.CardOwnerId = {0} AND inv.CardType = {1}" +
                                "GROUP BY inv.CardId";
                            string command = string.Format(format, playerID, "Character");
                            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                JsonData rowJson = new JsonData();
                                rowJson["CardId"] = Convert.ToInt32(row["CardId"]);
                                rowJson["CardName"] = Convert.ToString(row["CardName"]);
                                rowJson["OwnNum"] = Convert.ToInt32(row["OwnNum"]);
                                rowJson["MaxTalent"] = Convert.ToInt32(row["Talent"]);
                                rowJson["CardRarity"] = Convert.ToInt32(row["CardRarity"]);
                                rowJson["BaseHealth"] = Convert.ToInt32(row["BaseHealth"]);
                                rowJson["BaseEnergy"] = Convert.ToInt32(row["BaseEnergy"]);
                                rowJson["BaseAttack"] = Convert.ToInt32(row["BaseAttack"]);
                                rowJson["BaseSpeed"] = Convert.ToInt32(row["BaseSpeed"]);
                                rowJson["GrowHealth"] = Convert.ToInt32(row["GrowHealth"]);
                                rowJson["GrowEnergy"] = Convert.ToInt32(row["GrowEnergy"]);
                                rowJson["GrowAttack"] = Convert.ToInt32(row["GrowAttack"]);
                                rowJson["GrowSpeed"] = Convert.ToInt32(row["GrowSpeed"]);

                                json.Add(rowJson);
                            }

                            returnData = json.ToJson();

                            break;
                        }
                    case 4:
                        {
                            break;
                        }
                    default:
                        {
                            LogsSystem.Instance.Print("请求访问的类型非法非法:" + type, LogLevel.WARN);
                            returnCode = ReturnCode.Failed;//请求失败
                            return null;
                        }
                }
                data.returnData = returnData;
            }
            else
            {
                LogsSystem.Instance.Print("UUID非法:" + playerUUID, LogLevel.WARN);
                returnCode = ReturnCode.Failed;//请求失败
                return null;
            }

            //重新封装
            SocketModel model = new SocketModel();
            model.returnCode = returnCode;
            model.protocol = SocketProtocol.INVINFO;
            model.message = JsonCoding<InvInfoDTO>.encode(data);

            return model;
        }

        /// <summary>
        /// 返回断线信息包
        /// </summary>
        /// <returns></returns>
        private SocketModel Offline()
        {
            SocketModel model = new SocketModel();
            model.returnCode = ReturnCode.Refuse;
            model.protocol = SocketProtocol.OFFLINE;

            return model;
        }
    }
}
