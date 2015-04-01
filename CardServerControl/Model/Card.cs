using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model
{
    class Card
    {
        public int cardId;
        public string cardName;
        public int cardRarity;

        public Card()
        {

        }
        public Card(int id,string name,int rarity)
        {
            this.cardId = id;
            this.cardName = name;
            this.cardRarity = rarity;
        }
    }
}
