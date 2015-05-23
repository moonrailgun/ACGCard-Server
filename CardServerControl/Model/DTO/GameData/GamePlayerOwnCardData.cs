using System.Collections.Generic;

namespace CardServerControl.Model.DTO.GameData
{
    /// <summary>
    /// 卡片背包
    /// </summary>
    class GamePlayerOwnCardData : GameDetailData
    {
        public List<CardInfo> cardInv;
    }
}