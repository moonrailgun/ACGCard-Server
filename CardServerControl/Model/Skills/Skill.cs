using CardServerControl.Model.Cards;
using System;

namespace CardServerControl.Model.Skills
{
    abstract class Skill : ICloneable
    {
        public int skillEnergyCost = 0;//技能能量消耗

        public Skill(int skillID, string skillName)
        {
            this.skillID = skillID;
            this.skillName = skillName;
        }

        public int skillID;//技能ID
        public string skillName = "";

        public abstract void OnUse(PlayerCard from, PlayerCard target);

        /// <summary>
        /// 生成技能附加数据
        /// </summary>
        public abstract string GenerateSkillAppendData(PlayerCard from, PlayerCard to);

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
