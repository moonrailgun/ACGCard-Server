using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Skills
{
    /// <summary>
    /// 技能管理类
    /// </summary>
    class SkillManager
    {
        List<Skill> skillList;

        public SkillManager()
        {
            this.Register();
        }

        /// <summary>
        /// 技能注册
        /// </summary>
        private void Register()
        {

        }

        /// <summary>
        /// 根据技能ID查找技能
        /// </summary>
        public Skill GetSkillById(int skillID)
        {
            foreach (Skill skill in skillList)
            {
                if (skill.skillID == skillID)
                {
                    return skill;
                }
            }

            LogsSystem.Instance.Print("找不到该ID的技能", LogLevel.WARN);
            return null;
        }
    }
}
