using CardServerControl.Model;
using CardServerControl.Model.DTO;
using System;
using System.Data;
using System.Net;

namespace CardServerControl.Util
{
    class PacketProcess
    {
        public SocketModel LoginPacket(LoginDTO data, IPEndPoint ip)
        {
            SocketModel model = new SocketModel();
            model.areaCode = AreaCode.Server;
            model.protocol = SocketProtocol.LOGIN;
            model.message = JsonCoding<LoginDTO>.encode(data);

            string username = data.username;
            string password = data.password;

            //查询数据库
            string command = string.Format("SELECT count(*) FROM account WHERE Account = '{0}' AND Password = '{1}'", username, password);
            DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

            if (PlayerManager.Instance.CanLogin())
            {
                if (Convert.ToInt32(ds.Tables[0].Rows[0]["count(*)"]) != 0)
                {
                    //登陆成功
                    LogsSystem.Instance.Print(string.Format("账户{0}[{1}]已登录到系统", username, ip.Address.ToString()));
                    model.returnCode = ReturnCode.Success;

                    //为数据表创建uuid并写入
                    string uuid = System.Guid.NewGuid().ToString();
                    command = string.Format("UPDATE account SET UUID = '{0}',LastLogin = '{1}' WHERE Account = '{2}' AND Password = '{3}'", uuid, CommonDTO.GetTimeStamp().ToString(), username, password);
                    MySQLHelper.ExecuteNonQuery(MySQLHelper.Conn, CommandType.Text, command, null);

                    //添加到服务器的用户列表
                    PlayerManager.Instance.PlayerLogin(username, uuid, ip.Address.ToString());
                }
                else
                {
                    //登陆失败
                    LogsSystem.Instance.Print(string.Format("账户{0}[{1}]试图登陆游戏失败：用户名或密码错误", username, ip.Address.ToString()));
                    model.returnCode = ReturnCode.Failed;
                }
            }
            else
            {
                //服务器已满
                LogsSystem.Instance.Print(string.Format("账户{0}[{1}]试图登陆游戏失败：服务器已满", username, ip.Address.ToString()));
                model.returnCode = ReturnCode.Refuse;
            }

            return model;
        }

        public SocketModel ChatPacket(ChatDTO data)
        {
            string content = data.content;
            string senderUUID = data.senderUUID;
            string toUUID = data.toUUID;

            foreach (Player player in PlayerManager.Instance.GetPlayerList())
            {
                if (player.UUID != senderUUID)
                {
                    SocketModel chatmodel = new SocketModel();
                    chatmodel.areaCode = AreaCode.Server;
                    chatmodel.protocol = SocketProtocol.CHAT;
                    chatmodel.message = JsonCoding<ChatDTO>.encode(new ChatDTO(content, senderUUID));

                    UdpServer.Instance.SendToPlayerByUUID(JsonCoding<SocketModel>.encode(chatmodel), player.UUID);
                }
            }

            return null;//不返回数据包
        }
    }
}
