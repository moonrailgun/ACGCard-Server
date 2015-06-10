using CardServerControl.Model.Cards;
using LitJson;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    class AttackUp : StateSkill
    {
        protected int value;//攻击上升的值

        public AttackUp(int skillID, string skillName, int lastRound, int addValue)
            : base(skillID, skillName, lastRound)
        {
            this.value = addValue;
        }

        public override void OnUse(PlayerCard from, PlayerCard target)
        {
            this.stateOwnerCard = target;

            target.AddState(this);
        }

        /// <summary>
        /// 获取增加的攻击力值
        /// </summary>
        public int GetAddedValue()
        { return this.value; }

        /// <summary>
        /// 重设攻击增加信息（用于多样化）
        /// </summary>
        public void ResetAttackUpData(int lastRound, int addValue)
        {
            this.lastRound = this.allLastRound = lastRound;
            this.value = addValue;
        }

        public override string GenerateSkillAppendData()
        {
            JsonData json = new JsonData();
            json["value"] = this.value;
            json["lastRound"] = this.lastRound;
            json["allLastRound"] = this.allLastRound;

            return json.ToJson();
        }
    }
}
