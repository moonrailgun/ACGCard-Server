using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class GameRequestDTO : CommonDTO
    {
        public string playerUUID;
        public int playerUid;
        public string playerName;
        public int playerLevel;
    }
}
