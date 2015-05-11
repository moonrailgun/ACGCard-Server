/// <summary>
/// 断开连接
/// </summary>
namespace CardServerControl.Model.DTO
{
    class DisconnectDTO : CommonDTO
    {
        public string UUID ="";
        public int uid = -1;
        public int protocol = -1;
    }

    enum LinkProtocol
    {
        UDP = 0,
        TCP = 1
    }
}

