using CardServerControl.Model.Cards;
using System;

namespace CardServerControl.Model.Skills
{
    abstract class Skill : ICloneable
    {
        public Skill(int skillID, string skillName)
        {
            this.skillID = skillID;
            this.skillName = skillName;
        }

        public int skillID;//技能ID
        public string skillName = "";

        public abstract void OnUse(PlayerCard from, PlayerCard target);

        /// <summary>
        /// 获取技能附加数据
        /// </summary>
        public abstract string GetSkillAppendData();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
