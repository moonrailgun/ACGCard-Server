using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO.GameData
{
    class UseSkillData : GameDetailData
    {
        public int skillID;
        public string skillCommonName;//技能通用名
        public string fromCardUUID;
        public string toCardUUID;//可以为空
        public string skillAppendData;//技能附加信息
    }
}
