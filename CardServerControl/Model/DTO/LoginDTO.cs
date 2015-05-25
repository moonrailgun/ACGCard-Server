using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class LoginDTO : CommonDTO
    {
        public string account;
        public string password;
        public string playerName;
        public string UUID;

        public string internalVersion;//内部版本号
        public string officialVersion;//正式版本号

        public LoginDTO()
            :base()
        {

        }

        public LoginDTO(string username,string password)
            :base()
        {
            this.account = username;
            this.password = password;
        }
    }
}
