using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model
{
    //就是socket类型
    class SocketProtocol
    {
        public const int OFFLINE = 404;
        public const int LOGIN = 10;
        public const int CHAT = 11;
        public const int PLAYERINFO = 12;
        public const int CARDINFOLIST = 13;
    }
}