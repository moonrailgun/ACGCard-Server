using CardServerControl.Model.Cards;

namespace CardServerControl.Model.Skills
{
    abstract class Skill
    {
        public string skillName = "";

        public abstract void OnUse(PlayerCard from, PlayerCard target);
    }
}
