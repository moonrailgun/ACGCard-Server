using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace CardServerControl.Model.DTO.GameData
{
    class PlayerSocket
    {
        public PlayerInfoData playerInfo;
        public Socket socket;

        public PlayerSocket(PlayerInfoData info, Socket socket)
        {
            this.playerInfo = info;
            this.socket = socket;
        }
    }
}
