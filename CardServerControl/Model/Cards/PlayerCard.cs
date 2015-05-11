using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Cards
{
    class PlayerCard : Card
    {
        public int cardOwnerId;
        public int cardLevel;
        public int specialHealth;
        public int specialEnergy;
        public int specialAttack;
        public int specialSpeed;

        #region 属性获取
        public int GetHealth()
        {
            int value = this.baseHealth + this.growHealth * this.cardLevel + this.specialHealth;
            return value;
        }
        public int GetEnergy()
        {
            int value = this.baseEnergy + this.growEnergy * this.cardLevel + this.specialEnergy;
            return value;
        }
        public int GetAttack()
        {
            int value = this.baseAttack + this.growAttack * this.cardLevel + this.specialAttack;
            return value;
        }
        public int GetSpeed()
        {
            int value = this.baseSpeed + this.growSpeed * this.cardLevel + this.specialSpeed;
            return value;
        }
        #endregion

        public CardInfo GetCardInfo()
        {
            CardInfo info = new CardInfo();
            info.cardId = this.cardId;
            info.cardName = this.cardName;
            info.cardOwnerId = this.cardOwnerId;
            info.cardRarity = this.cardRarity;
            info.health = this.GetHealth();
            info.energy = this.GetSpeed();
            info.attack = this.GetAttack();
            info.speed = this.GetSpeed();

            return info;
        }
    }
}
