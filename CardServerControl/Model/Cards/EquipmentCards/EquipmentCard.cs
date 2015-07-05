using System;
using System.Collections.Generic;
using LitJson;

namespace CardServerControl.Model.Cards.EquipmentCards
{
    class EquipmentCard : ItemCard
    {
        protected PlayerCard EquipedCard;//装备了该装备的英雄

        /// <summary>
        /// 当装备被装备时
        /// </summary>
        public virtual void OnEquiped(PlayerCard playerCard)
        {
            this.EquipedCard = playerCard;

            //修改需要通告网络
        }

        /// <summary>
        /// 当装备被卸下时
        /// </summary>
        public virtual void OnUnequiped()
        {
            //修改需要通告网络
        }

        public string GenerateEquipAppend()
        {
            JsonData json = new JsonData();

            //----这里实现装备附加值的添加


            return json.ToJson();
        }
    }
}
