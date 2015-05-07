using CardServerControl.Model;
using CardServerControl.Model.DTO;
using CardServerControl.Model.DTO.GameData;
using System;
using System.Data;
using System.Net.Sockets;

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
        public GameData ProcessTcpData(int operateCode, string operateData, Socket socket)
        {
            switch (operateCode)
            {
                case OperateCode.Identify:
                    {
                        string UUID = operateData;
                        return ProcessIdentify(UUID, socket);
                    }
                default:
                    {
                        break;
                    }
            }
            return null;
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
    }
}
