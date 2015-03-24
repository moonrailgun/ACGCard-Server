using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.DTO
{
    class LoginDTO : CommonDTO
    {
        public string username;
        public string password;

        public LoginDTO()
            :base()
        {

        }

        public LoginDTO(string username,string password)
            :base()
        {
            this.username = username;
            this.password = password;
        }
    }
}
