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
        public int SpecialHealth;
        public int SpecialMana;
        public int SpecialAttack;
    }
}
