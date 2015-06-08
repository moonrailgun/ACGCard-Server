using CardServerControl.Model.Cards;
using LitJson;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    abstract class StateSkill : Skill
    {
        protected int lastRound;//可以持续的回合数
        protected int allLastRound;//总共可以持续的回合数,0为无限使用
        protected PlayerCard stateOwnerCard;//该状态的所有者

        protected StateSkill(int skillID, string skillName, int lastRound)
            : base(skillID, skillName)
        {
            this.allLastRound = this.allLastRound = lastRound;
        }

        /// <summary>
        /// 当角色攻击时
        /// </summary>
        protected virtual void OnCharacterAttack() { }

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
        protected virtual void OnRoundEnd() { }
        protected virtual void OnOtherSkillUse() { }

        /// <summary>
        /// 获取剩余的回合数
        /// </summary>
        public int GetRemainRounds()
        {
            return this.lastRound;
        }

        /// <summary>
        /// 设置状态基本信息
        /// </summary>
        public void SetStateInfo(PlayerCard stateOwnerCard)
        {
            this.stateOwnerCard = stateOwnerCard;
        }

        public override string GenerateSkillAppendData()
        {
            JsonData json = new JsonData();
            json["lastRound"] = this.lastRound;
            json["allLastRound"] = this.allLastRound;

            return json.ToJson();
        }

        /// <summary>
        /// 销毁该状态
        /// （状态自毁）
        /// </summary>
        public void DestoryState()
        {
            this.stateOwnerCard.RemoveState(this);
        }
    }
}