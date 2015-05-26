using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Util;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace CardServerControl
{
    /// <summary>
    /// TCP连接心跳检测
    /// 可能会消耗大量资源
    /// 后期可以考虑优化
    /// </summary>
    class HeartBeatSystem
    {
        #region 单例模式
        private static HeartBeatSystem _instance;
        public static HeartBeatSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HeartBeatSystem();
                }
                return _instance;
            }
        }
        #endregion

        Dictionary<PlayerSocket, Timer> HeartBeatList = new Dictionary<PlayerSocket, Timer>();//心跳包发送字典列表

        /// <summary>
        /// 添加发送心跳包的对象
        /// </summary>
        public void AddHeartBeatObject(PlayerSocket target)
        {
            if (!HeartBeatList.ContainsKey(target))
            {
                Timer timer = new Timer(SendHeartBeatPackage, (object)target, 0, 10 * 1000);

                HeartBeatList.Add(target, timer);//添加到队列
            }
        }

        /// <summary>
        /// 移除发送心跳包的对象
        /// </summary>
        public void RemoveHeartBeatObject(PlayerSocket target)
        {
            if (HeartBeatList.ContainsKey(target))
            {
                HeartBeatList[target].Dispose();//释放资源
                HeartBeatList.Remove(target);//移除队列
            }
        }

        /// <summary>
        /// 计时器回调函数
        /// </summary>
        private void SendHeartBeatPackage(object target)
        {
            if (target is PlayerSocket)
            {
                PlayerSocket playerSocket = target as PlayerSocket;

                GameData data = new GameData();
                data.operateCode = OperateCode.HeartBeat;
                data.operateData = JsonCoding<CommonDTO>.encode(new CommonDTO());

                TcpServer.Instance.Send(playerSocket.socket, data);
            }
            else
            {
                LogsSystem.Instance.Print("计时器回调函数接受数据异常，计时器无法正常工作:" + target, LogLevel.ERROR);
            }
        }

        /// <summary>
        /// 处理返回的心跳包
        /// </summary>
        public void ProcessHeartBeatPackage(Socket target)
        {
            throw new System.NotImplementedException();
        }
    }
}
