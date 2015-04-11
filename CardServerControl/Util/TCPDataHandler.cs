using CardServerControl.Util;
using CardServerControl.Model.DTO;
using CardServerControl.Model.DTO.GameData;
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
        public GameDataDTO ProcessTcpData(int operateCode, string operateData,Socket socket)
        {
            switch (operateCode)
            {
                case GameDataDTO.OperateCode.Identify:
                    {
                        PlayerInfoData info = JsonCoding<PlayerInfoData>.decode(operateData);
                        return ProcessIdentify(info, socket);
                    }
                default:
                    {
                        break;
                    }
            }
            return null;
        }

        private GameDataDTO ProcessIdentify(PlayerInfoData data,Socket socket)
        {
            //将数据和socket绑定
            TcpServer.Instance.GetGameRoomManager().BindSocket(data, socket);

            return null;
        }
    }
}
