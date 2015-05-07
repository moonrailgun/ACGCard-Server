namespace CardServerControl.Model
{
    /// <summary>
    /// TCP用协议
    /// 传输协议
    /// </summary>
    public class OperateCode
    {
        public const int Identify = 99;
        public const int AllocRoom = 30;
        public const int Attack = 31;
        public const int UseSkill = 32;
    }
}