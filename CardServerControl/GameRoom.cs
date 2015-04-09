using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl
{
    class GameRoom
    {
        public int roomID;
        public Socket socketA;
        public Socket socketB;

        public GameRoom()
        {

        }
        public GameRoom(int roomID,Socket socketA,Socket socketB)
        {
            this.roomID = roomID;
            this.socketA = socketA;
            this.socketB = socketB;
        }
    }
}
