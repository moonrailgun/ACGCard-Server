﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CardServerControl.Util;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Model.Skills;

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
        private SkillManager skillManager;
        private TCPDataHandler tcpDH;
        private TCPDataSender tcpSD;
        public const int gamePort = 28283;
        private TcpListener listener;

        /// <summary>
        /// 初始化TCP
        /// </summary>
        public void Init()
        {
            try
            {
                LogsSystem.Instance.Print("正在初始化TCP服务");
                grm = new GameRoomManager();
                skillManager = new SkillManager();
                tcpDH = new TCPDataHandler();
                tcpSD = new TCPDataSender();
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), gamePort);
                listener.Start();
                listener.BeginAcceptTcpClient(AcceptTcpClient, listener);//开始异步接受TCP连接
                LogsSystem.Instance.Print("TCP连接创建完毕，监听端口：" + gamePort);
            }
            catch (Exception ex)
            {
                LogsSystem.Instance.Print("TCP服务创建失败：" + ex.ToString(), LogLevel.ERROR);
            }

        }

        /// <summary>
        /// 异步接受tcp连接
        /// </summary>
        private void AcceptTcpClient(IAsyncResult ar)
        {
            try
            {
                LogsSystem.Instance.Print("有新的连接连入");
                TcpListener listener = (TcpListener)ar.AsyncState;

                TcpClient client = listener.EndAcceptTcpClient(ar);

                //开始异步接受数据
                Receive(client.Client);

                //加入未知连接队列
                grm.AddUnknownSocket(client.Client);

                //继续下一轮接受
                listener.BeginAcceptTcpClient(AcceptTcpClient, listener);
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

        /// <summary>
        /// 获取tcp数据发送器
        /// </summary>
        public TCPDataSender GetTCPDataSender()
        { return this.tcpSD; }

        /// <summary>
        /// 获取技能管理器
        /// </summary>
        public SkillManager GetSkillManager()
        { return this.skillManager; }

        #region 发送数据
        public void Send(Socket socket, GameData data)
        {
            string sendMessage = JsonCoding<GameData>.encode(data);
            byte[] sendBytes = encoding.GetBytes(sendMessage);
            Send(socket, sendBytes);
        }
        public void Send(Socket socket, byte[] data)
        {
            LogsSystem.Instance.Print(string.Format("TCP 发送数据({0}):{1}", data.Length, encoding.GetString(data)));
            if (socket.Connected && socket != null)
            {
                socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                LogsSystem.Instance.Print(string.Format("TCP 数据({0})发送完成", bytesSent));
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
                if (client.Connected)
                {
                    StateObject state = new StateObject();
                    state.socket = client;
                    client.BeginReceive(state.buffer, 0, StateObject.buffSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
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
                    foreach (byte b in receiveState.buffer)
                    {
                        if (b != 0x00)
                        {
                            //将缓存加入结果列
                            receiveState.dataByte.Add(b);
                        }
                    }
                    receiveState.buffer = new byte[StateObject.buffSize];//清空缓存

                    //接受完成
                    byte[] receiveBytes = receiveState.dataByte.ToArray();
                    //处理数据
                    ProcessReceiveMessage(receiveBytes, receiveState.socket);

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
        private void ProcessReceiveMessage(byte[] messageBytes, Socket socket)
        {
            if (messageBytes.Length > 0)
            {
                //获取收到的数据
                string message = encoding.GetString(messageBytes);
                LogsSystem.Instance.Print(string.Format("[TCP FROM {0}]:{1}", socket.RemoteEndPoint, message));

                //转换数据
                GameData data = JsonCoding<GameData>.decode(message);
                GameData returnData = tcpDH.ProcessTcpData(data, socket);
                if (returnData != null)
                {
                    string returnMessage = JsonCoding<GameData>.encode(returnData);
                    Send(socket, encoding.GetBytes(returnMessage));
                }
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
