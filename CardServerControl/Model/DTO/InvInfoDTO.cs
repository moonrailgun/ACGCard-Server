using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class InvInfoDTO : CommonDTO
    {
        public int playerID;
        public string playerUUID;
        public int type;//player = 1,Hero = 2,Guide = 3,Inv = 4
        public string returnData;
    }
}
