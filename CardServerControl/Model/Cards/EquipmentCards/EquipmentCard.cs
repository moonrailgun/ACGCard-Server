using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Cards.EquipmentCards
{
    class EquipmentCard : ItemCard
    {
        protected PlayerCard EquipedCard;//装备了该装备的英雄

        /// <summary>
        /// 当装备装备时
        /// </summary>
        public virtual void OnEquiped()
        {

        }

        /// <summary>
        /// 当卸下装备时
        /// </summary>
        public virtual void OnUnequiped()
        {

        }
    }
}
