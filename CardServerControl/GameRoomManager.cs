using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model.DTO;

namespace CardServerControl
{
    class GameRoomManager
    {
        public List<GameRoom> rooms;
        private int availableRoomID;
        //public List<GameRequestDTO> undistributedRequest;
        public List<Socket> undistributedSocket;

        public GameRoomManager()
        {
            rooms = new List<GameRoom>();
            availableRoomID = 0;
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        public GameRoom CreateRoom(Socket socketA, Socket socketB)
        {
            GameRoom newroom = new GameRoom(availableRoomID, socketA, socketB);
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
        public void AddUndistributedSocket(Socket socket)
        {
            undistributedSocket.Add(socket);
            TryDistributeRoom();
        }

        /// <summary>
        /// 分配房间
        /// </summary>
        private void TryDistributeRoom()
        {
            if (undistributedSocket.Count >= 2)
            {
                Socket socketA, socketB;

                //简单匹配，之后修改
                socketA = undistributedSocket[0];
                socketB = undistributedSocket[1];
                undistributedSocket.Remove(socketA);
                undistributedSocket.Remove(socketB);

                CreateRoom(socketA, socketB);//创建房间
            }
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
    }
}
