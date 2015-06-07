using CardServerControl.Model;
using CardServerControl.Model.Cards;
using CardServerControl.Model.DTO.GameData;
using System.Data;
using System.Collections.Generic;
using System;
using CardServerControl.Model.Skills;
using CardServerControl.Model.Skills.ContinuedSkill;

namespace CardServerControl.Util
{
    /// <summary>
    /// 服务端主动发送数据
    /// </summary>
    class TCPDataSender
    {
        /// <summary>
        /// 根据玩家的UUID和UID
        /// 发送玩家拥有的卡片(仅在此时创建卡片的UUID)
        /// </summary>
        public GameData SendPlayerOwnCard(GamePlayerOwnCardData requestData, GameRoom room)
        {
            int uid = requestData.operatePlayerUid;
            string UUID = requestData.operatePlayerUUID;
            GameRoom.PlayerPosition position = (GameRoom.PlayerPosition)requestData.operatePlayerPosition;

            if (CheckUUID(UUID))
            {
                string command;
                List<PlayerCard> playerCardList = new List<PlayerCard>();
                CardManager cardManager = CardManager.Instance;

                //获取背包信息
                requestData.cardInv = new List<CardInfo>();
                command = string.Format("SELECT * FROM cardinventory WHERE CardOwnerId = '{0}'", uid);
                DataSet cardInventory = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                foreach (DataRow row in cardInventory.Tables[0].Rows)
                {
                    try
                    {
                        int cardID = Convert.ToInt32(row["CardId"]);

                        CharacterCard standardCharacterCard = cardManager.GetCardCloneByID(cardID);

                        PlayerCard playerCard = new PlayerCard();
                        //卡片通用
                        playerCard.cardUUID = System.Guid.NewGuid().ToString();//创建卡片的UUID作为唯一标识
                        playerCard.cardId = cardID;
                        playerCard.cardName = standardCharacterCard.cardName;
                        playerCard.cardRarity = standardCharacterCard.cardRarity;
                        //角色卡通用
                        playerCard.baseHealth = standardCharacterCard.baseHealth;
                        playerCard.baseEnergy = standardCharacterCard.baseEnergy;
                        playerCard.baseAttack = standardCharacterCard.baseAttack;
                        playerCard.baseSpeed = standardCharacterCard.baseSpeed;
                        playerCard.growHealth = standardCharacterCard.growHealth;
                        playerCard.growEnergy = standardCharacterCard.growEnergy;
                        playerCard.growAttack = standardCharacterCard.growAttack;
                        playerCard.growSpeed = standardCharacterCard.growSpeed;
                        //游戏卡个性化
                        playerCard.cardOwnerId = Convert.ToInt32(row["CardOwnerId"]);
                        playerCard.cardLevel = Convert.ToInt32(row["CardLevel"]);
                        playerCard.specialHealth = Convert.ToInt32(row["SpecialHealth"]);
                        playerCard.specialEnergy = Convert.ToInt32(row["SpecialEnergy"]);
                        playerCard.specialAttack = Convert.ToInt32(row["SpecialAttack"]);
                        playerCard.specialSpeed = Convert.ToInt32(row["SpecialSpeed"]);
                        playerCard.InitHealthAndEnergy();

                        //设置卡片拥有技能
                        int[] skillIdArray = IntArray.StringToIntArray(row["CardOwnSkill"].ToString());//获取卡片技能的ID列表
                        SkillManager skillManager = TcpServer.Instance.GetSkillManager();
                        foreach (int skillId in skillIdArray)
                        {
                            Skill skill = skillManager.GetSkillById(skillId);//从技能管理器中获取该技能的克隆
                            playerCard.AddSkill(skill);
                        }

                        CardInfo cardInfo = playerCard.GetCardInfo();
                        requestData.cardInv.Add(cardInfo);//添加到列表
                        playerCardList.Add(playerCard);//添加卡片
                    }
                    catch (Exception ex)
                    {
                        LogsSystem.Instance.Print(ex.ToString(), LogLevel.WARN);
                    }
                }

                //将数据加载到本地内存
                room.SetCardInv(playerCardList, position);

                //封装返回数据
                GameData returnData = new GameData();
                returnData.operateCode = OperateCode.PlayerOwnCard;
                returnData.roomID = room.roomID;
                returnData.returnCode = ReturnCode.Success;
                returnData.operateData = JsonCoding<GamePlayerOwnCardData>.encode(requestData);

                return returnData;
            }
            else
            {
                return Offline();
            }
        }

        /// <summary>
        /// 向客户端发送状态修改操作的请求
        /// </summary>
        public void SendStateOperate(StateSkill skill, PlayerCard operateCard, int operateType, int operatePosition, GameRoom gameRoom)
        {
            OperateStateData detail = new OperateStateData();
            detail.skillID = skill.skillID;
            detail.stateOperate = operateType;
            switch (operateCard.GetCardOwnerPosition())
            {
                case 0://A
                    {
                        detail.operatePlayerPosition = 0;
                        detail.operatePlayerUid = gameRoom.playerSocketA.playerInfo.playerUid;
                        detail.operatePlayerUUID = gameRoom.playerSocketA.playerInfo.playerUUID;
                        break;
                    }
                case 1://B
                    {
                        detail.operatePlayerPosition = 1;
                        detail.operatePlayerUid = gameRoom.playerSocketB.playerInfo.playerUid;
                        detail.operatePlayerUUID = gameRoom.playerSocketB.playerInfo.playerUUID;
                        break;
                    }
                default:
                    {
                        LogsSystem.Instance.Print("错误:卡片拥有者的位置出现错误:" + operateCard.GetCardOwnerPosition(), LogLevel.WARN);
                        break;
                    }
            }

            GameData data = new GameData();
            data.returnCode = ReturnCode.Success;
            data.roomID = gameRoom.roomID;
            data.operateCode = OperateCode.OperateState;
            data.operateData = JsonCoding<OperateStateData>.encode(detail);

            gameRoom.SendOperateToAllPlayer(data);
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