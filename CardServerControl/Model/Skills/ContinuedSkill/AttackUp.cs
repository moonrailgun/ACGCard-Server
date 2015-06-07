using CardServerControl.Model.Cards;
using LitJson;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    class AttackUp : StateSkill
    {
        protected int value;

        public AttackUp(int skillID, string skillName, int lastRound, int addValue)
            :base(skillID, skillName, lastRound)
        {
            this.value = addValue;
        }

        public override void OnUse(PlayerCard from, PlayerCard target)
        {
            this.stateOwnerCard = target;

            target.AddState(this);
        }

        public override string GenerateSkillAppendData(Cards.PlayerCard from, Cards.PlayerCard to)
        {
            JsonData json = new JsonData();
            json["value"] = this.value;
            json["lastRound"] = this.lastRound;
            json["allLastRound"] = this.allLastRound;

            return json.ToJson();
        }
    }
}
