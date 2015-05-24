using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardServerControl.Model.Skills.ContinuedSkill
{
    abstract class StateSkill : Skill
    {
        protected abstract void OnCharacterAttack();
        protected abstract void OnRoundStart();
        protected abstract void OnRoundEnd();
        protected abstract void OnOtherSkillUse();
    }
}