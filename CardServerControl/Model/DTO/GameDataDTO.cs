using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    //游戏数据通用
    //TCP数据传输
    class GameDataDTO
    {
        public int roomID;//房间名
        public int returnCode;//返回值
        public int operateCode;//游戏操作名
        public string operateData;//操作数据

        public static class OperateCode
        {
            public const int Identify = 99;
            public const int AllocRoom = 30;
            public const int Attack = 31;
            public const int UseSkill = 32;
        }
    }
}
