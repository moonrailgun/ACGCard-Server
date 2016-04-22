using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CardServerControl
{
    class UdpServer
    {
        #region 单例模式
        private static UdpServer _instance;
        public static UdpServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UdpServer();
                }
                return _instance;
            }
        }
        #endregion

        public UdpClient receiveClient;
        public UdpClient sendClient;
        private const int localPort = 23333;//服务端端口
        private const int remotePort = 22233;//客户端默认端口
        private LogsSystem log;
        private Thread thread;
        public PacketProcess pp;//处理类
        public CardManager cardManager;//卡片管理类

        public UdpServer()
        {
            this.receiveClient = new UdpClient(localPort);
            this.sendClient = new UdpClient();
            log = LogsSystem.Instance;
            pp = new PacketProcess();
            cardManager = CardManager.Instance;
            cardManager.Load();//读取卡片数据（更新）
        }

        public void Connect()
        {
            try
            {
                log.Print("[服务器]正在创建服务器连接...");
                if (thread == null)
                {
                    thread = new Thread(new ThreadStart(Listen));
                    thread.Start();
                }
                else
                {
                    log.Print("[服务器]服务器创建失败:服务器已经被创建", LogLevel.WARN);
                }
            }
            catch (Exception ex)
            {
                log.Print("[服务器]服务器创建失败，发生异常：" + ex, LogLevel.ERROR);
            }
        }

        /// <summary>
        /// 监听线程
        /// </summary>
        private void Listen()
        {
            log.Print("[服务器]开始监听端口：" + localPort);
            this.thread.IsBackground = true;//将线程设为后台线程,当前台线程全部关闭后会自动关闭后台线程

            while (true)
            {
                //接受
                IPEndPoint remoteEP = null;
                byte[] receivedInf = receiveClient.Receive(ref remoteEP);
                string Text = Encoding.UTF8.GetString(receivedInf);
                log.Print(string.Format("[远程{0}]{1}", remoteEP.ToString(), Text));

                //响应，根据发送的端口返回
                byte[] response = ResponsePacket(Text, remoteEP);
                if (response != null)
                {
                    sendClient.Send(response, response.Length, remoteEP);
                    log.Print(string.Format("[发送至{0}]{1}", remoteEP.ToString(), Encoding.UTF8.GetString(response)));
                }

            }
        }

        public void StopListen()
        {
            if (this.thread != null)
            {
                log.Print("[服务器]关闭监听端口：" + localPort);
                this.thread.Abort();
                log.Print("[线程]" + this.thread.ThreadState.ToString());
                this.thread = null;
                PlayerManager.Instance.ClearPlayerList();
            }
            else
            {
                log.Print("[服务器]失败:服务器已经被关闭", LogLevel.WARN);
            }
        }

        /// <summary>
        /// 发送数据包到远程
        /// </summary>
        /// <param name="message"></param>
        public void SendMsg(string message, string hostname, int port = remotePort)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            sendClient.Send(bytes, bytes.Length, hostname, port);
        }

        /// <summary>
        /// 发送数据包给固定UUID的对象
        /// </summary>
        public void SendToPlayerByUUID(string message, string UUID)
        {
            foreach (Player player in PlayerManager.Instance.GetLobbyPlayerList())
            {
                if (player.UUID == UUID)
                {
                    SendMsg(message, player.IPed.Address.ToString(),player.IPed.Port);
                }
            }
        }


        /// <summary>
        /// 处理数据并根据数据返回相应的数据包
        /// </summary>
        /// <param name="text">收到的文本</param>
        /// <param name="toIped">发送给对方的IP地址</param>
        /// <returns>数据包</returns>
        private byte[] ResponsePacket(string text, IPEndPoint toIped)
        {
            SocketModel model = JsonCoding<SocketModel>.decode(text);
            int areacode = model.areaCode;
            int returncode = model.returnCode;
            int protocol = model.protocol;
            string message = model.message;

            SocketModel returnModel = new SocketModel();
            switch (protocol)
            {
                case SocketProtocol.LOGIN:
                    {
                        returnModel = pp.LoginPacket(JsonCoding<LoginDTO>.decode(message), toIped);
                        break;
                    }
                case SocketProtocol.CHAT:
                    {
                        returnModel = pp.ChatPacket(JsonCoding<ChatDTO>.decode(message));
                        break;
                    }
                case SocketProtocol.PLAYERINFO:
                    {
                        returnModel = pp.PlayerInfoPacket(JsonCoding<PlayerInfoDTO>.decode(message));
                        break;
                    }
                case SocketProtocol.CARDINFOLIST:
                    {
                        returnModel = pp.CardInfoPacket(JsonCoding<CardInfoDTO>.decode(message));
                        break;
                    }
                case SocketProtocol.INVINFO:
                    {
                        returnModel = pp.InvInfoPacket(JsonCoding<InvInfoDTO>.decode(message));
                        break;
                    }
                default:
                    {
                        LogsSystem.Instance.Print("接收到未知的数据包:" + text, LogLevel.WARN);
                        break;
                    }
            }

            //将model转码成二进制
            if (returnModel != null)
            {
                return Encoding.UTF8.GetBytes(JsonCoding<SocketModel>.encode(returnModel));//返回给客户端信息
            }
            else
            {
                return null;//不返回客户端信息
            }
        }

        /// <summary>
        /// 发送给所有在线玩家
        /// </summary>
        /// <param name="messageByte"></param>
        public void SendToAllPlayer(byte[] messageByte)
        {
            List<Player> playerList = PlayerManager.Instance.GetLobbyPlayerList();
            foreach (Player player in playerList)
            {
                //-------------------------------注意这里是同步发送所以后期可能需要修改
                sendClient.Send(messageByte, messageByte.Length, player.IPed.Address.ToString(), player.IPed.Port);
            }
        }
    }
}
