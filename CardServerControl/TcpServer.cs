using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CardServerControl
{
    class TcpServer
    {
        //编码格式
        Encoding encoding = Encoding.ASCII;







        #region 发送数据
        private void Send(Socket socket, byte[] data)
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
        private void Receive(Socket client)
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
        #endregion

        /// <summary>
        /// 处理接受到的数据
        /// </summary>
        /// <param name="message">接收到的数据</param>
        /// <param name="fromPort">接受的本地端口</param>
        private void ProcessReceiveMessage(byte[] messageBytes, EndPoint localEndPoint, Socket socket)
        {
            //获取收到的数据
            string message = encoding.GetString(messageBytes);

            //-------处理


        }

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
