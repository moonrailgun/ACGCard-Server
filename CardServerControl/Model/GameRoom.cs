using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model.DTO.GameData;

namespace CardServerControl.Model
{
    class GameRoom
    {
        public int roomID;
        public PlayerSocket playerSocketA;
        public PlayerSocket playerSocketB;
        public GamePlayerData playerDataA;
        public GamePlayerData playerDataB;

        public GameRoom()
        {

        }
        public GameRoom(int roomID, PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            this.roomID = roomID;
            this.playerSocketA = playerSocketA;
            this.playerSocketB = playerSocketB;
        }

        /// <summary>
        /// 设置卡片背包列表
        /// </summary>
        public void SetCardInv(List<CardInfo> cardInv ,PlayerPosition position)
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
        /// 游戏中玩家的信息
        /// </summary>
        class GamePlayerData
        {
            public List<CardInfo> cardInv;//卡片背包
        }

        public enum PlayerPosition
        { A, B }
    }
}
