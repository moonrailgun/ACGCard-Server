using CardServerControl.Model.Cards;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    abstract class StateSkill : Skill
    {
        protected int lastRound;//可以持续的回合数
        protected int allLastRound;//总共可以持续的回合数,0为无限使用
        protected PlayerCard ownerCard;//该状态的所有者

        protected StateSkill(int skillID, string skillName, int lastRound)
            :base(skillID, skillName)
        {
            this.allLastRound = this.allLastRound = lastRound;
        }

        /// <summary>
        /// 当角色攻击时
        /// </summary>
        protected abstract void OnCharacterAttack();

        /// <summary>
        /// 回合开始
        /// </summary>
        protected virtual void OnRoundStart()
        {
            if (allLastRound != 0)
            {
                lastRound--;//持续回合递减
                if (lastRound <= 0)
                {
                    DestoryState();
                }
            }
        }
        protected abstract void OnRoundEnd();
        protected abstract void OnOtherSkillUse();

        /// <summary>
        /// 销毁该状态
        /// （状态自毁）
        /// </summary>
        public void DestoryState()
        {
            this.ownerCard.RemoveState(this);
        }
    }
}