using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * TODO
 * 添加对所有玩家时时刻刻发送消息检测是否在线的机制
 */
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

        public void PlayerLogin(string username,string UUID,string ip)
        {
            Player player = new Player();
            player.username = username;
            player.UUID = UUID;
            player.IPAddress = ip;

            this.playerList.Add(player);
        }

        public List<Player> GetPlayerList()
        {
            return this.playerList;
        }

        /// <summary>
        /// 返回现在玩家人数
        /// </summary>
        public int GetPlayerNumber()
        {
            return playerList.Count;
        }

        /// <summary>
        /// 返回是否能登陆
        /// true：可以登陆
        /// false：不能登陆，服务器已满
        /// </summary>
        /// <returns></returns>
        public bool CanLogin()
        {
            return GetPlayerNumber() < maxPlayerNumber;
        }

        /// <summary>
        /// 根据UUID获取玩家姓名
        /// </summary>
        public string GetPlayerNameByUUID(string uuid)
        {
            foreach (Player player in playerList)
            {
                if (player.UUID == uuid)
                {
                    return player.username;
                }
            }
            return "";
        }

        public void ClearPlayerList()
        {
            playerList.Clear();
        }
    }
}
