using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class ChatDTO : CommonDTO
    {
        public string content;
        public string senderName;
        public string senderUUID;
        public string toUUID;

        public ChatDTO()
            : base()
        {

        }

        public ChatDTO(string content, string senderName, string senderUUID, string toUUID = "")
            : base()
        {
            this.content = content;
            this.senderName = senderName;
            this.senderUUID = senderUUID;
            this.toUUID = toUUID;
        }
    }
}
