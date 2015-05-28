using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Model.Cards;

namespace CardServerControl.Model
{
    class GameRoom
    {
        public int roomID;
        public PlayerSocket playerSocketA;
        public PlayerSocket playerSocketB;
        public GamePlayerData playerDataA;
        public GamePlayerData playerDataB;

        public GameRoom(int roomID, PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            this.roomID = roomID;
            this.playerSocketA = playerSocketA;
            this.playerSocketB = playerSocketB;

            this.playerDataA = new GamePlayerData();
            this.playerDataB = new GamePlayerData();
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

        /// <summary>
        /// 根据卡片UUID获取场上的卡片
        /// </summary>
        public PlayerCard GetPlayerCard(string cardUUID)
        {
            if (playerDataA.characterCard.ContainsKey(cardUUID))
            {
                return playerDataA.characterCard[cardUUID];
            }
            else if (playerDataB.characterCard.ContainsKey(cardUUID))
            {
                return playerDataB.characterCard[cardUUID];
            }
            else
            {
                LogsSystem.Instance.Print("场上找不要这张卡:" + cardUUID, LogLevel.WARN);
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

            /// <summary>
            /// 初始化
            /// </summary>
            public GamePlayerData()
            {
                this.characterCard = new Dictionary<string, PlayerCard>();
                this.handCard = new List<CardInfo>();
                this.characterCardInv = new List<PlayerCard>();
            }

            public void AddPlayerCard(PlayerCard card)
            {
                characterCard.Add(card.cardUUID, card);
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
            /// 根据UUID返回角色卡片
            /// </summary>
            public PlayerCard GetPlayerCardByCardUUID(string CardUUID)
            {
                return characterCard[CardUUID];
            }
        }

        public enum PlayerPosition
        { A = 0, B = 1 }
    }
}
