using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Model.DTO.GameData;
using System;
using System.Data;
using System.Net.Sockets;
using System.Net;

namespace CardServerControl.Util
{
    class TCPDataHandler
    {
        /// <summary>
        /// 对外接口
        /// 处理所有的TCP数据
        /// 并返回相应
        /// 如果返回null则不发送
        /// </summary>
        public GameData ProcessTcpData(GameData data, Socket socket)
        {
            int operateCode = data.operateCode;
            string operateData = data.operateData;
            switch (operateCode)
            {
                case OperateCode.Identify:
                    {
                        string UUID = operateData;
                        if (CheckUUID(UUID))
                        {
                            return ProcessIdentify(UUID, socket);
                        }
                        else
                        {
                            return Offline();
                        }
                    }
                case OperateCode.Offline:
                    {
                        return ProcessCancelMatching(JsonCoding<DisconnectDTO>.decode(data.operateData), socket);
                    }
                default:
                    {
                        break;
                    }
            }
            return null;
        }

        /// <summary>
        /// 检查UUID是否合法
        /// </summary>
        private bool CheckUUID(string uuid)
        {
            string command = string.Format("SELECT * FROM account WHERE UUID = '{0}'", uuid);
            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
            if (ds.Tables[0].Rows.Count > 0)//UUID验证通过
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据UUID做
        /// 身份验证处理
        /// </summary>
        private GameData ProcessIdentify(string uuid, Socket socket)
        {
            try
            {
                PlayerInfoData playerInfo = new PlayerInfoData();
                playerInfo.playerUUID = uuid;

                string command = string.Format("SELECT * FROM account WHERE UUID = '{0}'", uuid);
                DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                if (ds.Tables[0].Rows.Count > 0)//UUID验证通过
                {
                    //获取UID
                    int uid = Convert.ToInt32(ds.Tables[0].Rows[0]["Uid"]);
                    playerInfo.playerUid = uid;

                    //获取玩家名字
                    command = string.Format("SELECT * FROM playerinfo WHERE Uid = '{0}'", uid);
                    ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);
                    playerInfo.playerName = ds.Tables[0].Rows[0]["PlayerName"].ToString();

                    //绑定
                    TcpServer.Instance.GetGameRoomManager().BindSocket(playerInfo, socket);

                    //添加入玩家管理
                    PlayerManager.Instance.PlayerLoginGameServer(playerInfo.playerUid, playerInfo.playerName, playerInfo.playerUUID, socket);
                }
                else
                {
                    LogsSystem.Instance.Print("未知的UUID试图绑定连接", LogLevel.WARN);
                }
            }
            catch (Exception ex)
            {
                LogsSystem.Instance.Print("发生异常" + ex.ToString(), LogLevel.ERROR);
            }
            return null;
        }

        /// <summary>
        /// 取消匹配|主动发送断线请求
        /// 清除房间列表和玩家列表后返回断线信息完成断线
        /// </summary>
        private GameData ProcessCancelMatching(DisconnectDTO data, Socket socket)
        {
            //服务端断线
            if (data.protocol == (int)LinkProtocol.TCP)
            {
                //断开游戏，不退出大厅
                PlayerManager.Instance.GamePlayerLogout(data.uid, socket);
            }

            return null;
        }

        /// <summary>
        /// 返回断线信息
        /// 用于把用户踢出
        /// </summary>
        private GameData Offline()
        {
            GameData data = new GameData();
            data.operateCode = OperateCode.Offline;
            data.returnCode = ReturnCode.Refuse;

            return data;
        }
    }
}
