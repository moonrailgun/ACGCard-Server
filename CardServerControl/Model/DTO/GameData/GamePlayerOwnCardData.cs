using System.Collections.Generic;
using CardServerControl.Model.Cards;

namespace CardServerControl.Model.DTO.GameData
{
    /// <summary>
    /// 卡片背包
    /// </summary>
    class GamePlayerOwnCardData : CommonDTO
    {
        public int uid;
        public List<PlayerCard> cardInv;

        public GamePlayerOwnCardData()
            : base()
        {
            cardInv = new List<PlayerCard>();
        }
    }
}
