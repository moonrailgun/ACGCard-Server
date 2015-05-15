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
        public void SetCardInv(List<CardInfo> cardInv, PlayerPosition position)
        {
            if (position == PlayerPosition.A)//A
            {
                playerDataA.cardInv = cardInv;
            }
            else//B
            {
                playerDataB.cardInv = cardInv;
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
        /// 游戏中玩家的信息
        /// </summary>
        public class GamePlayerData
        {
            public Dictionary<CardInfo, string> characterCard;//场上卡片<卡片对象，卡片UUID>
            public List<CardInfo> handCard;//手牌
            public List<CardInfo> cardInv;//卡片背包

            public void AddCharacterCard(CardInfo card)
            {
                characterCard.Add(card, card.cardUUID);
            }

            public bool IsOwnCard(string UUID)
            {
                foreach (CardInfo info in cardInv)
                {
                    if (info.cardUUID == UUID)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public enum PlayerPosition
        { A, B }
    }
}
