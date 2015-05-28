using CardServerControl.Model.Cards;

namespace CardServerControl.Model.Skills
{
    abstract class Skill
    {
        public Skill(int skillID, string skillName)
        {
            this.skillID = skillID;
            this.skillName = skillName;
        }

        public int skillID;//技能ID
        public string skillName = "";

        public abstract void OnUse(PlayerCard from, PlayerCard target);
    }
}
