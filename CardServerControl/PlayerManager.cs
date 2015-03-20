using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl
{
    class PlayerManager
    {
        #region 单例模式
        private static PlayerManager _instance;
        public static PlayerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PlayerManager();
                }
                return _instance;
            }
        }
        #endregion

        private List<Player> playerList;//服务器玩家列表
        public int maxPlayerNumber;//服务器最高人数

        private PlayerManager()
        {
            playerList = new List<Player>();
            maxPlayerNumber = 20;
        }

        public void PlayerLogin(string username,string UUID)
        {
            Player player = new Player();
            player.username = username;
            player.UUID = UUID;

            this.playerList.Add(player);
        }

        public List<Player> GetPlayerList()
        {
            return this.playerList;
        }
    }
}
