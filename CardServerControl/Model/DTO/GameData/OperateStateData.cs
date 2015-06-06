namespace CardServerControl.Model.DTO.GameData
{
    class OperateStateData : GameDetailData
    {
        public int skillID;
        public int stateOperate;
        public string appendData;

        public class StateOperateCode
        {
            public const int AddState = 5;
            public const int RemoveState = 6;
            public const int UpdateState = 7;
        }
    }
}
