using System.Collections.Generic;

namespace CardServerControl.Model.DTO.GameData
{
    /// <summary>
    /// 卡片背包
    /// </summary>
    class GamePlayerOwnCardData : CommonDTO
    {
        public int playerUid;
        public List<CardInfo> cardInv;
    }
}