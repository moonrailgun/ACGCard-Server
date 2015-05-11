using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using CardServerControl.Model.DTO;
using CardServerControl.Model;
using CardServerControl.Util;
using CardServerControl.Model.DTO.GameData;
using System.Net.Sockets;

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

        private List<Player> lobbyPlayerList;//大厅服务器玩家列表
        private List<PlayerSocket> gamePlayerList;//游戏服务器玩家列表
        public int maxPlayerNumber;//服务器最高人数

        private PlayerManager()
        {
            lobbyPlayerList = new List<Player>();
            gamePlayerList = new List<PlayerSocket>();
            maxPlayerNumber = 20;
        }
        /// <summary>
        /// 玩家登陆大厅
        /// </summary>
        public void PlayerLoginLobby(int uid, string playerName, string UUID, IPEndPoint iped)
        {
            //检查玩家是否已经登陆。如果已登陆则踢出之前的
            foreach (Player onlinePlayer in lobbyPlayerList)
            {
                if (onlinePlayer.uid == uid && onlinePlayer.UUID != UUID)
                {
                    //踢出大厅
                    SocketModel model = new SocketModel();
                    model.protocol = SocketProtocol.OFFLINE;
                    model.returnCode = ReturnCode.Refuse;

                    UdpServer.Instance.SendMsg(JsonCoding<SocketModel>.encode(model), onlinePlayer.IPed.Address.ToString(), onlinePlayer.IPed.Port);//发送断线消息
                    this.lobbyPlayerList.Remove(onlinePlayer);

                    //踢出游戏
                    foreach (PlayerSocket gameOnlinePlayer in gamePlayerList)
                    {
                        if (gameOnlinePlayer.playerInfo.playerUid == onlinePlayer.uid)
                        {
                            GameData data = new GameData();
                            data.operateCode = OperateCode.Offline;
                            data.returnCode = ReturnCode.Refuse;

                            TcpServer.Instance.Send(gameOnlinePlayer.socket, data);
                            this.gamePlayerList.Remove(gameOnlinePlayer);
                            break;//离开游戏玩家列表遍历循环
                        }
                    }
                    break;//离开大厅玩家列表遍历循环
                }
            }

            //登陆系统
            Player player = new Player();
            player.uid = uid;
            player.playerName = playerName;
            player.UUID = UUID;
            player.IPed = iped;

            this.lobbyPlayerList.Add(player);

            //登陆成功
            LogsSystem.Instance.Print(string.Format("玩家{0}[{1}]已登录到游戏", playerName, iped.Address.ToString()));
        }

        /// <summary>
        /// 玩家登陆游戏服务器
        /// </summary>
        public void PlayerLoginGameServer(int uid, string playerName, string UUID, Socket socket)
        {
            //检查玩家是否已经登陆。如果已登陆则踢出之前的
            foreach (PlayerSocket onlinePlayer in gamePlayerList)
            {
                string onlinePlayerUUID = onlinePlayer.playerInfo.playerUUID;
                if (onlinePlayerUUID == UUID)
                {
                    GameData data = new GameData();
                    data.operateCode = OperateCode.Offline;
                    data.returnCode = ReturnCode.Refuse;

                    TcpServer.Instance.Send(onlinePlayer.socket, data);

                    //TcpServer.Instance.
                    //UdpServer.Instance.SendMsg(JsonCoding<GameData>.encode(model), onlinePlayer.IPed.Address.ToString(), onlinePlayer.IPed.Port);//发送断线消息
                    this.gamePlayerList.Remove(onlinePlayer);
                }
            }

            //登陆系统
            PlayerInfoData pid = new PlayerInfoData();
            pid.playerUid = uid;
            pid.playerName = playerName;
            pid.playerUUID = UUID;

            PlayerSocket ps = new PlayerSocket(pid, socket);
            this.gamePlayerList.Add(ps);
        }

        public List<Player> GetLobbyPlayerList()
        {
            return this.lobbyPlayerList;
        }

        /// <summary>
        /// 返回现在在游戏中玩家人数
        /// </summary>
        public int GetGamePlayerNumber()
        {
            return this.gamePlayerList.Count;
        }

        /// <summary>
        /// 返回大厅玩家
        /// </summary>
        public int GetLobbyPlayerNumber()
        { return this.lobbyPlayerList.Count; }

        /// <summary>
        /// 返回是否能登陆
        /// true：可以登陆
        /// false：不能登陆，服务器已满
        /// </summary>
        /// <returns></returns>
        public bool CanLogin()
        {
            return GetGamePlayerNumber() < maxPlayerNumber;
        }

        /// <summary>
        /// 根据UUID获取玩家姓名
        /// </summary>
        public string GetPlayerNameByUUID(string uuid)
        {
            Player player = GetLobbyPlayerByUUID(uuid);
            if (player != null)
            {
                return player.playerName;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 根据玩家姓名获取UUID
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetLobbyPlayerUUIDByName(string username)
        {
            foreach (Player player in lobbyPlayerList)
            {
                if (player.playerName == username)
                {
                    return player.UUID;
                }
            }
            return "";
        }

        /// <summary>
        /// 根据玩家uid获取玩家对象
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public Player GetLobbyPlayerByUid(int uid)
        {
            foreach (Player player in lobbyPlayerList)
            {
                if (player.uid == uid)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据UUID获取玩家对象
        /// </summary>
        public Player GetLobbyPlayerByUUID(string uuid)
        {
            foreach (Player player in lobbyPlayerList)
            {
                if (player.UUID == uuid)
                {
                    return player;
                }
            }
            return null;
        }

        public void ClearPlayerList()
        {
            gamePlayerList.Clear();
        }

        /// <summary>
        /// 大厅玩家登出
        /// </summary>
        public void LobbyPlayerLogout(int uid)
        {
            foreach (Player player in lobbyPlayerList)
            {
                if (player.uid == uid)
                {
                    string command = string.Format("UPDATE account SET UUID = '' WHERE Uid = '{0}'", uid);
                    MySQLHelper.ExecuteNonQuery(MySQLHelper.Conn, CommandType.Text, command, null);
                    return;
                }
            }
        }
        public void LobbyPlayerLogout(string uuid)
        {
            Player player = GetLobbyPlayerByUUID(uuid);
            if (player != null)
            {
                LobbyPlayerLogout(player.uid);
            }
        }

        /// <summary>
        /// 游戏玩家退出
        /// </summary>
        public void GamePlayerLogout(int uid,Socket socket)
        {
            //清除列表
            foreach (PlayerSocket player in gamePlayerList)
            {
                if (player.playerInfo.playerUid == uid)
                {
                    gamePlayerList.Remove(player);
                    break;
                }
            }
            //离开游戏房间
            TcpServer.Instance.GetGameRoomManager().LeaveRoom(uid,socket);
        }
    }
}
