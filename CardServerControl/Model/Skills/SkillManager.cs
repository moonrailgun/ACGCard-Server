using System;
using System.Collections.Generic;
using CardServerControl.Model.Skills.InstantSkill;
using CardServerControl.Model.Skills.ContinuedSkill;

namespace CardServerControl.Model.Skills
{
    /// <summary>
    /// 技能管理类
    /// 后期添加反射机制用于扩展，前期直接写死
    /// </summary>
    class SkillManager
    {
        List<Skill> skillList = new List<Skill>();

        public SkillManager()
        {
            this.Register();
        }

        /// <summary>
        /// 技能注册
        /// </summary>
        private void Register()
        {
            //伤害数据为测试用，并非最终数据
            AddSkill(new AttackSkill(1, "ArcaneMissiles", 60, 50));
            AddSkill(new AttackSkill(2, "Fireball", 50, 40));
            AddSkill(new AttackSkill(3, "FireArrow", 70, 60));
            AddSkill(new AttackSkill(4, "Meteorites", 70, 60));
            AddSkill(new AttackSkill(5, "Thunderbolt", 90, 80));
            AddSkill(new AttackSkill(6, "MeteoriteCut", 80, 70));
            AddSkill(new AttackUp(7, "AttackUp", 3, 50));

            LogsSystem.Instance.Print("技能数据加载完毕,共有 " + skillList.Count + " 个技能");
        }

        private void AddSkill(Skill skill)
        {
            this.skillList.Add(skill);
        }

        /// <summary>
        /// 根据技能ID获取技能的克隆
        /// </summary>
        public Skill GetSkillById(int skillID)
        {
            foreach (Skill skill in skillList)
            {
                if (skill.skillID == skillID)
                {
                    return skill.Clone() as Skill;
                }
            }

            LogsSystem.Instance.Print("找不到该ID的技能", LogLevel.WARN);
            return null;
        }
    }
}
