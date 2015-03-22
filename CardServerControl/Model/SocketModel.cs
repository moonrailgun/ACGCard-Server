using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model
{
    class SocketModel
    {
        public int areaCode { get; set; }
        public int returnCode { get; set; }
        public int protocol { get; set; }
        public string message { get; set; }
    }
}
