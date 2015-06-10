using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    class WeaponAttackUp : AttackUp
    {
        public WeaponAttackUp(int skillID, string skillName, int lastRound, int addValue)
            : base(skillID, skillName, lastRound, addValue)
        { }

        protected override void OnRoundStart()
        {
            //清空回合开始事件
        }

        /// <summary>
        /// 当角色攻击时，造成伤害
        /// </summary>
        protected override void OnCharacterAttack()
        {
            base.OnCharacterAttack();

            if (allLastRound != 0)
            {
                lastRound--;//持续回合递减
                if (lastRound <= 0)
                {
                    DestoryState();
                }
            }
        }
    }
}
