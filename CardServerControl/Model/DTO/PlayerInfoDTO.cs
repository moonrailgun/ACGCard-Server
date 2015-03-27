using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class PlayerInfoDTO
    {
        public string UUID;

        public int uid;
        public string playerName;
        public int level;
        public int coin;
        public int gem;
        public DateTime vipExpire;

        public PlayerInfoDTO()
            : base()
        {

        }
    }
}
