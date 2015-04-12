using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CardServerControl.Model.DTO;
using CardServerControl.Util;

namespace CardServerControl
{
    class TcpServer
    {
        #region 单例模式
        private static TcpServer _instance;
        public static TcpServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TcpServer();
                }
                return _instance;
            }
        }
        #endregion
        //编码格式
        public static Encoding encoding = Encoding.UTF8;
        private GameRoomManager grm;
        private TCPDataHandler tcpDH;
        public const int gamePort = 28283;
        private TcpListener listener;

        /// <summary>
        /// 初始化TCP
        /// </summary>
        public void Init()
        {
            LogsSystem.Instance.Print("正在初始化TCP服务");
            grm = new GameRoomManager();
            tcpDH = new TCPDataHandler();
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), gamePort);
            listener.Start();
            listener.BeginAcceptTcpClient(AcceptTcpClient, listener);//开始异步接受TCP连接
            LogsSystem.Instance.Print("TCP连接创建完毕，监听端口：" + gamePort);
        }

        /// <summary>
        /// 异步接受tcp连接
        /// </summary>
        private void AcceptTcpClient(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                if (listener.Server.Connected)//如果监听器有效
                {
                    TcpClient client = listener.EndAcceptTcpClient(ar);

                    //开始异步接受数据
                    Receive(client.Client);

                    //加入未知连接队列
                    grm.AddUnknownSocket(client.Client);

                    //继续下一轮接受
                    listener.BeginAcceptTcpClient(AcceptTcpClient, listener);
                }
            }
            catch (Exception ex)
            {
                LogsSystem.Instance.Print(ex.ToString(), LogLevel.ERROR);
            }

        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void StopListen()
        {
            if (listener != null)
            {
                try
                {
                    listener.Stop();
                    grm.CloseGame();//关闭连接
                    LogsSystem.Instance.Print("已关闭TCP服务");
                }
                catch (Exception ex)
                {
                    LogsSystem.Instance.Print("关闭TCP失败" + ex, LogLevel.ERROR);
                }
            }
        }

        /// <summary>
        /// 获取游戏房间管理器
        /// </summary>
        public GameRoomManager GetGameRoomManager()
        {
            return this.grm;
        }

        #region 发送数据
        public void Send(Socket socket,GameDataDTO data)
        {
            string sendMessage = JsonCoding<GameDataDTO>.encode(data);
            byte[] sendBytes = encoding.GetBytes(sendMessage);
            Send(socket, sendBytes);
        }
        public void Send(Socket socket, byte[] data)
        {
            LogsSystem.Instance.Print(string.Format("发送数据({0}):{1}", data.Length, encoding.GetString(data)));
            socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                LogsSystem.Instance.Print(string.Format("数据({0})发送完成", bytesSent));
            }
            catch (Exception e)
            {
                LogsSystem.Instance.Print(e.ToString(), LogLevel.ERROR);
            }
        }
        #endregion

        #region 接受数据
        public void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.socket = client;
                client.BeginReceive(state.buffer, 0, StateObject.buffSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                LogsSystem.Instance.Print(e.ToString(), LogLevel.ERROR);
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject receiveState = (StateObject)ar.AsyncState;
                Socket client = receiveState.socket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead < StateObject.buffSize)
                {
                    //如果读取到数据长度较小
                    receiveState.dataByte.AddRange(receiveState.buffer);//将缓存加入结果列
                    receiveState.buffer = new byte[StateObject.buffSize];//清空缓存

                    //接受完成
                    byte[] receiveData = receiveState.dataByte.ToArray();
                    LogsSystem.Instance.Print(string.Format("接受到{0}字节数据", receiveData.Length));
                    //处理数据
                    ProcessReceiveMessage(receiveData, client.LocalEndPoint, receiveState.socket);

                    Receive(client);//继续下一轮的接受
                }
                else
                {
                    //如果读取到数据长度大于缓冲区
                    receiveState.dataByte.AddRange(receiveState.buffer);//将缓存加入结果列
                    receiveState.buffer = new byte[StateObject.buffSize];//清空缓存
                    client.BeginReceive(receiveState.buffer, 0, StateObject.buffSize, 0, new AsyncCallback(ReceiveCallback), receiveState);//继续接受下一份数据包
                }
            }
            catch (Exception e)
            {
                LogsSystem.Instance.Print(e.ToString(), LogLevel.ERROR);
            }
        }

        /// <summary>
        /// 处理接受到的数据
        /// </summary>
        /// <param name="message">接收到的数据</param>
        /// <param name="fromPort">接受的本地端口</param>
        private void ProcessReceiveMessage(byte[] messageBytes, EndPoint localEndPoint, Socket socket)
        {
            //获取收到的数据
            string message = encoding.GetString(messageBytes);

            //转换数据
            GameDataDTO data = JsonCoding<GameDataDTO>.decode(message);
            GameDataDTO returnData = tcpDH.ProcessTcpData(data.operateCode, data.operateData, socket);
            if (returnData != null)
            {
                string returnMessage = JsonCoding<GameDataDTO>.encode(returnData);
                Send(socket, encoding.GetBytes(returnMessage));
            }
        }
        #endregion
    }

    class StateObject
    {
        //socket 客户端
        public Socket socket = null;
        //缓冲区大小
        public const int buffSize = 256;
        //缓冲
        public byte[] buffer = new byte[buffSize];
        //数据流
        public List<byte> dataByte = new List<byte>();
    }
}
