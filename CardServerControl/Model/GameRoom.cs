using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Model.Cards;
using CardServerControl.Util;

namespace CardServerControl.Model
{
    class GameRoom
    {
        public int roomID;
        public PlayerSocket playerSocketA;
        public PlayerSocket playerSocketB;
        public GamePlayerData playerDataA;
        public GamePlayerData playerDataB;

        public bool isWaitGameStart = true;
        public bool isPlayerReadyA = false;
        public bool isPlayerReadyB = false;

        public PlayerPosition roundOwner;//当前回合的玩家位置

        public GameRoom(int roomID, PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            this.roomID = roomID;
            this.playerSocketA = playerSocketA;
            this.playerSocketB = playerSocketB;

            this.playerDataA = new GamePlayerData(this, PlayerPosition.A);
            this.playerDataB = new GamePlayerData(this, PlayerPosition.B);
        }

        /// <summary>
        /// 设置卡片背包列表
        /// </summary>
        public void SetCardInv(List<PlayerCard> cardInv, PlayerPosition position)
        {
            if (position == PlayerPosition.A)//A
            {
                playerDataA.characterCardInv = cardInv;
            }
            else//B
            {
                playerDataB.characterCardInv = cardInv;
            }
        }

        /// <summary>
        /// 将数据包发送给房间内两方玩家
        /// 常用于数据操作
        /// </summary>
        /// <param name="data"></param>
        public void SendOperateToAllPlayer(GameData data)
        {
            TcpServer.Instance.Send(playerSocketA.socket, data);
            TcpServer.Instance.Send(playerSocketB.socket, data);
        }

        /// <summary>
        /// 根据位置标识获取对应位置的玩家数据信息
        /// </summary>
        public GamePlayerData GetPlayerDataByPositionSign(int position)
        {
            if (position == (int)PlayerPosition.A)
            {
                return this.playerDataA;
            }
            else if (position == (int)PlayerPosition.B)
            {
                return this.playerDataB;
            }
            else
            {
                LogsSystem.Instance.Print("未知的位置标示，无法获取玩家数据信息", LogLevel.WARN);
                return null;
            }
        }
        public GamePlayerData GetPlayerDataByPositionSign(PlayerPosition position)
        {
            return GetPlayerDataByPositionSign((int)position);
        }

        private int GetOppositePositionSign(int position)
        {
            return (int)GetOppositePositionSign((PlayerPosition)position);
        }
        private PlayerPosition GetOppositePositionSign(PlayerPosition position)
        {
            if (position == PlayerPosition.A)
            {
                return PlayerPosition.B;
            }
            else
            {
                return PlayerPosition.A;
            }
        }

        /// <summary>
        /// 根据卡片UUID获取场上的卡片
        /// </summary>
        public PlayerCard GetPlayerCard(string cardUUID)
        {
            if (playerDataA.isHavePlayerCard(cardUUID))
            {
                return playerDataA.GetPlayerCardByCardUUID(cardUUID);
            }
            else if (playerDataB.isHavePlayerCard(cardUUID))
            {
                return playerDataB.GetPlayerCardByCardUUID(cardUUID);
            }
            else
            {
                LogsSystem.Instance.Print("场上找不到这张卡:" + cardUUID, LogLevel.WARN);
                return null;
            }
        }

        /// <summary>
        /// 游戏中玩家的信息
        /// </summary>
        public class GamePlayerData
        {
            public Dictionary<string, PlayerCard> characterCard;//场上卡片<卡片UUID,卡片对象>
            public List<CardInfo> handCard;//手牌
            public List<PlayerCard> characterCardInv;//角色卡片背包
            public GameRoom gameRoom;//当前信息的上级房间信息索引
            public PlayerPosition position;//信息所在位置

            /// <summary>
            /// 初始化
            /// </summary>
            public GamePlayerData(GameRoom room, PlayerPosition position)
            {
                this.characterCard = new Dictionary<string, PlayerCard>();
                this.handCard = new List<CardInfo>();
                this.characterCardInv = new List<PlayerCard>();
                this.gameRoom = room;
                this.position = position;
            }

            /// <summary>
            /// 添加卡片到房间
            /// </summary>
            public void AddPlayerCard(PlayerCard card)
            {
                characterCard.Add(card.cardUUID, card);
                card.SetOwnerData(this.gameRoom, (int)this.position);
            }

            public bool IsOwnCard(string UUID)
            {
                foreach (PlayerCard info in characterCardInv)
                {
                    if (info.cardUUID == UUID)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// 检查是否存在这张卡
            /// </summary>
            public bool isHavePlayerCard(string cardUUID)
            {
                if (characterCard.ContainsKey(cardUUID))
                {
                    if (characterCard[cardUUID].isAlive)
                    { return true; }
                    else
                    {
                        LogsSystem.Instance.Print("卡片已死亡，立即从队列中删除" + cardUUID, LogLevel.WARN);
                        characterCard.Remove(cardUUID);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// 根据UUID返回角色卡片
            /// </summary>
            public PlayerCard GetPlayerCardByCardUUID(string cardUUID)
            {
                if (characterCard.ContainsKey(cardUUID))
                {
                    if (characterCard[cardUUID].isAlive)
                    { return characterCard[cardUUID]; }
                    else
                    {
                        LogsSystem.Instance.Print("卡片已死亡，立即从队列中删除" + cardUUID, LogLevel.ERROR);
                        characterCard.Remove(cardUUID);
                        return null;
                    }
                }
                else
                {
                    LogsSystem.Instance.Print("不存在这张卡:" + cardUUID, LogLevel.ERROR);
                    return null;
                }
            }
            public List<PlayerCard> GetAllPlayerCard()
            {
                List<PlayerCard> playerCard = new List<PlayerCard>();
                foreach (KeyValuePair<string, PlayerCard> pair in this.characterCard)
                {
                    playerCard.Add(pair.Value);
                }
                return playerCard;
            }
        }

        public enum PlayerPosition
        { A = 0, B = 1 }

        #region 游戏玩家操作处理
        /// <summary>
        /// 添加角色卡到场上
        /// </summary>
        public void AddPlayerCardIntoStage(PlayerPosition position, string cardUUID, GameData data)
        {
            GamePlayerData gamePlayerData;
            if (position == PlayerPosition.A)
            {
                gamePlayerData = this.playerDataA;
            }
            else
            {
                gamePlayerData = this.playerDataB;
            }
            if (gamePlayerData.IsOwnCard(cardUUID))
            {
                foreach (PlayerCard playerCard in gamePlayerData.characterCardInv)
                {
                    if (cardUUID == playerCard.cardUUID)
                    {
                        gamePlayerData.AddPlayerCard(playerCard);//添加到英雄区
                        LogsSystem.Instance.Print(string.Format("{1}召唤{0}到场上", playerCard.cardName, position));
                        break;
                    }
                }
            }
            else
            {
                LogsSystem.Instance.Print("程序出现异常：没有找到该卡的UUID:" + cardUUID, LogLevel.ERROR);
            }
            SendOperateToAllPlayer(data);//转发信息


            //处理召唤
            if (isWaitGameStart)
            {
                //游戏尚未正式开始
                if (position == PlayerPosition.A)
                {
                    isPlayerReadyA = true;
                }
                else
                {
                    isPlayerReadyB = true;
                }

                if (isPlayerReadyA && isPlayerReadyB)
                {
                    GameStart();
                }
            }
        }
        /// <summary>
        /// 游戏正式开始
        /// </summary>
        public void GameStart()
        {
            LogsSystem.Instance.Print("游戏开始", LogLevel.GAMEDETAIL);
            isWaitGameStart = false;

            SendGameStart(PlayerPosition.A);
        }
        /// <summary>
        /// 设置当前回合所有者
        /// </summary>
        public void SendRoundSwitch(PlayerPosition position)
        {
            GamePlayerData playerData = GetPlayerDataByPositionSign(position);
            foreach (PlayerCard playerCard in playerData.GetAllPlayerCard())
            {
                //设置所有卡片可用
                playerCard.SetAvailable(true);
            }

            this.roundOwner = position;
            RoundSwtichData detail = new RoundSwtichData();
            detail.roundPosition = (int)position;

            GameData data = new GameData();
            data.roomID = this.roomID;
            data.operateCode = OperateCode.RoundSwitch;
            data.returnCode = ReturnCode.Success;
            data.operateData = JsonCoding<RoundSwtichData>.encode(detail);
            SendOperateToAllPlayer(data);//发送信息
        }
        /// <summary>
        /// 发送信息游戏开始.只能被调用一次
        /// </summary>
        public void SendGameStart(PlayerPosition position)
        {
            //可用玩家
            GamePlayerData playerData = GetPlayerDataByPositionSign(position);
            foreach (PlayerCard playerCard in playerData.GetAllPlayerCard())
            {
                //设置所有卡片可用
                playerCard.SetAvailable(true);
            }
            //不可用玩家
            GamePlayerData oppositePlayerData = GetPlayerDataByPositionSign(GetOppositePositionSign(position));
            foreach (PlayerCard playerCard in oppositePlayerData.GetAllPlayerCard())
            {
                //设置所有卡片不可用
                playerCard.SetAvailable(false);
            }

            this.roundOwner = position;
            RoundSwtichData detail = new RoundSwtichData();
            detail.roundPosition = (int)position;

            GameData data = new GameData();
            data.roomID = this.roomID;
            data.operateCode = OperateCode.GameStart;
            data.returnCode = ReturnCode.Success;
            data.operateData = JsonCoding<RoundSwtichData>.encode(detail);
            SendOperateToAllPlayer(data);//发送信息
        }
        #endregion
    }
}
