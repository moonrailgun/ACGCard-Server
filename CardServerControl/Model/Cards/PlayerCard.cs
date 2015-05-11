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
        public int specialHealth;
        public int specialMana;
        public int specialAttack;
    }
}
