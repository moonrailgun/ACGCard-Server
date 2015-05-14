namespace CardServerControl.Model.DTO.GameData
{
    /// <summary>
    /// 所有游戏内操作通用传输类
    /// </summary>
    class GameDetailData : CommonDTO
    {
        public int operatePlayerUid;
        public string operatePlayerUUID;
        public int operatePlayerPosition;//A = 0，B = 1
    }
}
