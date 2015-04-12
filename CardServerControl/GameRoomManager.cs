using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Util;

namespace CardServerControl
{
    class GameRoomManager
    {
        public List<GameRoom> rooms;
        private int availableRoomID;
        //public List<GameRequestDTO> undistributedRequest;
        public List<Socket> unknownSocket;
        public List<PlayerSocket> freedomPlayer;

        public GameRoomManager()
        {
            rooms = new List<GameRoom>();
            availableRoomID = 0;
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        public GameRoom CreateRoom(PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            GameRoom newroom = new GameRoom(availableRoomID, playerSocketA, playerSocketB);
            rooms.Add(newroom);
            availableRoomID++;

            //发送数据
            //--内容是分配到的房间ID,对手信息等数据

            return newroom;
        }

        /*
        /// <summary>
        /// 添加请求到未分配队列
        /// </summary>
        public void AddRequest(GameRequestDTO request)
        {
            if (!undistributedRequest.Contains(request))
                undistributedRequest.Add(request);
        }*/

        /// <summary>
        /// 把socket连接添加到房间
        /// </summary>
        /// <param name="socket"></param>
        public void AddUnknownSocket(Socket socket)
        {
            unknownSocket.Add(socket);//添加未知的连接

            //发送请求身份验证
            GameDataDTO data = new GameDataDTO();
            data.roomID = -1;
            data.returnCode = ReturnCode.Request;
            data.operateCode = GameDataDTO.OperateCode.Identify;

            TcpServer.Instance.Send(socket, data);
        }

        /// <summary>
        /// 分配房间
        /// </summary>
        private void TryAllocRoom()
        {
            if (unknownSocket.Count >= 2)
            {
                PlayerSocket playerSocketA, playerSocketB;

                //简单匹配，之后修改
                playerSocketA = freedomPlayer[0];
                playerSocketB = freedomPlayer[1];
                freedomPlayer.Remove(playerSocketA);
                freedomPlayer.Remove(playerSocketB);

                CreateRoom(playerSocketA, playerSocketB);//创建房间
            }
        }

        /// <summary>
        /// 清空所有房间
        /// </summary>
        public void CloseGame()
        {
            //关闭未知房间的连接
            foreach (Socket socket in unknownSocket)
            {
                socket.Close();
            }
            unknownSocket.Clear();

            //关闭自由玩家连接
            foreach (PlayerSocket playerSocket in freedomPlayer)
            {
                playerSocket.socket.Close();
            }
            freedomPlayer.Clear();

            //关闭房间中的连接
            foreach (GameRoom room in rooms)
            {
                room.socketA.Close();
                room.socketB.Close();
            }
            rooms.Clear();
        }

        /// <summary>
        /// 根据房间号获取房间对象
        /// </summary>
        public GameRoom GetRoom(int roomID)
        {
            foreach (GameRoom room in rooms)
            {
                if (room.roomID == roomID)
                {
                    return room;
                }
            }
            return null;
        }

        public void BindSocket(PlayerInfoData playerInfo, Socket socket)
        {
            try
            {
                int index = unknownSocket.IndexOf(socket);
                //添加到已绑定的socket列表
                PlayerSocket playerSocket = new PlayerSocket(playerInfo, socket);
                freedomPlayer.Add(playerSocket);

                //从未知连接列表中删除
                unknownSocket.Remove(socket);
            }
            catch(Exception ex)
            {
                LogsSystem.Instance.Print("绑定连接失败，可能是不存在该连接:"+ex.ToString(),LogLevel.WARN);
            }
        }
    }
}
