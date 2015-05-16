using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Cards
{
    /// <summary>
    /// 标准角色卡片属性
    /// </summary>
    class CharacterCard : Card
    {
        public int baseHealth;
        public int baseEnergy;
        public int baseAttack;
        public int baseSpeed;
        public int growHealth;
        public int growEnergy;
        public int growAttack;
        public int growSpeed;

        public CharacterCard()
        {

        }
        public CharacterCard(int id, string name, int rarity)
        {
            this.cardId = id;
            this.cardName = name;
            this.cardRarity = rarity;
        }
    }
}