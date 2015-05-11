using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Cards
{
    /// <summary>
    /// 标准卡片属性
    /// </summary>
    class Card: ICloneable
    {
        public int cardId;
        public string cardName;
        public int cardRarity;
        public int baseHealth;
        public int baseEnergy;
        public int baseAttack;
        public int baseSpeed;
        public int growHealth;
        public int growEnergy;
        public int growAttack;
        public int growSpeed;

        public Card()
        {

        }
        public Card(int id,string name,int rarity)
        {
            this.cardId = id;
            this.cardName = name;
            this.cardRarity = rarity;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
