using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model.DTO.GameData;

namespace CardServerControl
{
    class GameRoom
    {
        public int roomID;
        public PlayerSocket playerSocketA;
        public PlayerSocket playerSocketB;

        public GameRoom()
        {

        }
        public GameRoom(int roomID, PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            this.roomID = roomID;
            this.playerSocketA = playerSocketA;
            this.playerSocketB = playerSocketB;
        }
    }
}
