/// <summary>
/// 断开连接
/// </summary>
namespace CardServerControl.Model.DTO
{
    class DisconnectDTO : CommonDTO
    {
        public string UUID;
        public int uid;
        public int protocol;
    }

    enum LinkProtocol
    {
        UDP = 0,
        TCP = 1
    }
}

