using CardServerControl.Model;
using CardServerControl.Model.Cards;
using CardServerControl.Model.DTO.GameData;
using System.Data;
using System.Collections.Generic;
using System;

namespace CardServerControl.Util
{
    /// <summary>
    /// 服务端主动发送数据
    /// </summary>
    class TCPDataSender
    {
        /// <summary>
        /// 发送玩家拥有的卡片
        /// </summary>
        public GameData SendPlayerOwnCard(int uid, string UUID)
        {
            if (CheckUUID(UUID))
            {
                string command;
                CardManager cardManager = UdpServer.Instance.cardManager;

                //用户数据获取
                GamePlayerOwnCardData data = new GamePlayerOwnCardData();
                data.playerUid = uid;

                //获取背包信息
                data.cardInv = new List<PlayerCard>();
                command =  string.Format("SELECT * FROM cardinventory WHERE CardOwnerId = '{0}'",uid);
                DataSet cardInventory = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                foreach (DataRow row in cardInventory.Tables[0].Rows)
                {
                    try
                    {
                        PlayerCard playerCard = new PlayerCard();
                        playerCard.cardId = Convert.ToInt32(row["CardId"]);
                        playerCard.specialHealth = Convert.ToInt32(row["SpecialHealth"]);
                        playerCard.specialMana = Convert.ToInt32(row["SpecialMana"]);
                        playerCard.specialAttack = Convert.ToInt32(row["SpecialAttack"]);

                        //------------------------------------------在卡片管理器中寻找卡片标准数据(标准生命值什么的)，然后附加上玩家独立的数据

                        data.cardInv.Add(playerCard);//添加到队列
                        
                        throw new NotImplementedException();
                    }
                    catch (Exception ex)
                    {
                        LogsSystem.Instance.Print(ex.ToString(), LogLevel.WARN);
                    }
                }
                

                //封装
                GameData returnData = new GameData();
                returnData.operateCode = OperateCode.PlayerOwnCard;
                returnData.roomID = -1;
                returnData.returnCode = ReturnCode.Success;
                returnData.operateData = JsonCoding<GamePlayerOwnCardData>.encode(data);

                return returnData;
            }
            else
            {
                return Offline();
            }
        }


        /// <summary>
        /// 检查UUID是否合法
        /// </summary>
        public static bool CheckUUID(string uuid)
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
        /// 返回断线信息
        /// 用于把用户踢出
        /// </summary>
        public static GameData Offline()
        {
            GameData data = new GameData();
            data.operateCode = OperateCode.Offline;
            data.returnCode = ReturnCode.Refuse;

            return data;
        }
    }
}