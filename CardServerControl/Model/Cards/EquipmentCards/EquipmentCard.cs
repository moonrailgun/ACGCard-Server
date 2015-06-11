using System;
using System.Collections.Generic;
using LitJson;

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

        public string GenerateEquipAppend()
        {
            JsonData json = new JsonData();

            //----这里实现装备附加值的添加


            return json.ToJson();
        }
    }
}
