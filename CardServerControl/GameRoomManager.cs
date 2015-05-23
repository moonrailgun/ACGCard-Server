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
        public List<Socket> unknownSocket;
        public List<PlayerSocket> freedomPlayer;

        public GameRoomManager()
        {
            rooms = new List<GameRoom>();
            unknownSocket = new List<Socket>();
            freedomPlayer = new List<PlayerSocket>();
            availableRoomID = 0;
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        public GameRoom CreateRoom(PlayerSocket playerSocketA, PlayerSocket playerSocketB)
        {
            int roomID = availableRoomID;
            GameRoom newroom = new GameRoom(roomID, playerSocketA, playerSocketB);
            rooms.Add(newroom);
            availableRoomID++;
            
            /*
            //对playerA发送的卡片信息
            TcpServer.Instance.Send(playerSocketA.socket, TcpServer.Instance.GetTCPDataSender().SendPlayerOwnCard(newroom, GameRoom.PlayerPosition.A));
            //对playerB发送的卡片信息
            TcpServer.Instance.Send(playerSocketB.socket, TcpServer.Instance.GetTCPDataSender().SendPlayerOwnCard(newroom, GameRoom.PlayerPosition.B));
            */
            
            //发送数据,让客户端建立房间
            //通用数据
            GameData roomData = new GameData();
            roomData.operateCode = OperateCode.AllocRoom;
            roomData.returnCode = ReturnCode.Success;

            //对playerA发送的房间信息
            AllocRoomData roomDataToPlayerA = new AllocRoomData();
            roomDataToPlayerA.roomID = roomID;
            roomDataToPlayerA.allocPosition = AllocRoomData.Position.A;
            roomDataToPlayerA.rivalName = playerSocketB.playerInfo.playerName;
            roomDataToPlayerA.rivalUid = playerSocketB.playerInfo.playerUid;
            roomDataToPlayerA.rivalUUID = playerSocketB.playerInfo.playerUUID;
            string messageToA = JsonCoding<AllocRoomData>.encode(roomDataToPlayerA);

            //对playerB发送的房间信息
            AllocRoomData roomDataToPlayerB = new AllocRoomData();
            roomDataToPlayerB.roomID = roomID;
            roomDataToPlayerB.allocPosition = AllocRoomData.Position.B;
            roomDataToPlayerB.rivalName = playerSocketA.playerInfo.playerName;
            roomDataToPlayerB.rivalUid = playerSocketA.playerInfo.playerUid;
            roomDataToPlayerB.rivalUUID = playerSocketA.playerInfo.playerUUID;
            string messageToB = JsonCoding<AllocRoomData>.encode(roomDataToPlayerB);

            //对A发送信息
            roomData.operateData = messageToA;
            TcpServer.Instance.Send(playerSocketA.socket, roomData);

            //对B发送信息
            roomData.operateData = messageToB;
            TcpServer.Instance.Send(playerSocketB.socket, roomData);

            LogsSystem.Instance.Print(string.Format("房间{0}创建完毕；对战玩家[{1},{2}]", roomID, playerSocketA.playerInfo.playerName, playerSocketB.playerInfo.playerName));

            return newroom;
        }

        /// <summary>
        /// 把socket连接添加到房间
        /// </summary>
        /// <param name="socket"></param>
        public void AddUnknownSocket(Socket socket)
        {
            unknownSocket.Add(socket);//添加未知的连接

            //发送请求身份验证
            GameData data = new GameData();
            data.roomID = -1;
            data.returnCode = ReturnCode.Request;
            data.operateCode = OperateCode.Identify;

            TcpServer.Instance.Send(socket, data);
        }

        /// <summary>
        /// 分配房间
        /// </summary>
        private void TryAllocRoom()
        {
            LogsSystem.Instance.Print("尝试分配房间,当前等待人数:" + freedomPlayer.Count);
            if (freedomPlayer.Count >= 2)
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
            if (unknownSocket.Count > 0)
            {
                foreach (Socket socket in unknownSocket)
                {
                    socket.Close();
                }
                unknownSocket.Clear();
            }

            //关闭自由玩家连接
            if (freedomPlayer.Count > 0)
            {
                foreach (PlayerSocket playerSocket in freedomPlayer)
                {
                    playerSocket.socket.Close();
                }
                freedomPlayer.Clear();
            }

            //关闭房间中的连接
            if (rooms.Count > 0)
            {
                foreach (GameRoom room in rooms)
                {
                    room.playerSocketA.socket.Close();
                    room.playerSocketB.socket.Close();
                }
                rooms.Clear();
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

        /// <summary>
        /// 绑定连接
        /// </summary>
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

                //日志记录
                LogsSystem.Instance.Print(string.Format("绑定成功[{0},{1}]", playerInfo.playerName, socket.RemoteEndPoint.ToString()));

                //绑定后尝试分配房间
                TryAllocRoom();
            }
            catch (Exception ex)
            {
                LogsSystem.Instance.Print("绑定连接失败，可能是不存在该连接:" + ex.ToString(), LogLevel.WARN);
            }
        }

        /// <summary>
        /// 销毁一个房间
        /// </summary>
        public void DestroyRoom(int roomID)
        {
            foreach (GameRoom room in rooms)
            {
                if (room.roomID == roomID)
                {
                    rooms.Remove(room);
                    LogsSystem.Instance.Print("向两端发送房间销毁信息，未完成", LogLevel.ERROR);

                    return;
                }
            }
        }

        /// <summary>
        /// 玩家离开房间
        /// </summary>
        public void LeaveRoom(int uid, Socket socket)
        {
            //检查未验证队列
            foreach (Socket _socket in unknownSocket)
            {
                if (_socket == socket)
                {
                    unknownSocket.Remove(_socket);
                    _socket.Close();//关闭连接
                    return;
                }
            }

            //检查队列
            foreach (PlayerSocket player in freedomPlayer)
            {
                if (player.playerInfo.playerUid == uid)
                {
                    freedomPlayer.Remove(player);//清除列表
                    player.socket.Close();//关闭连接
                    LogsSystem.Instance.Print(string.Format("已关闭{0}连接", player.playerInfo.playerName));
                    return;
                }
            }

            //检查游戏房间
            foreach (GameRoom room in rooms)
            {
                if (room.playerSocketA.playerInfo.playerUid == uid)
                {
                    room.playerSocketA.socket.Close();//关闭连接
                    LogsSystem.Instance.Print("向B发送A断线消息，未完成", LogLevel.ERROR);
                    DestroyRoom(room.roomID);//销毁房间
                    return;
                }
                if (room.playerSocketB.playerInfo.playerUid == uid)
                {
                    room.playerSocketB.socket.Close();
                    LogsSystem.Instance.Print("向A发送B断线消息，未完成", LogLevel.ERROR);
                    DestroyRoom(room.roomID);
                    return;
                }
            }
        }
    }
}
