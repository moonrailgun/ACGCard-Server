using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Cards
{
    /// <summary>
    /// 卡片通用类
    /// </summary>
    class Card : ICloneable
    {
        public string cardUUID;
        public int cardId;
        public string cardName;
        public int cardRarity;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
