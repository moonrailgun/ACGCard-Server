using CardServerControl.Model.Skills.ContinuedSkill;
using CardServerControl.Model.Skills;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardServerControl.Util;

namespace CardServerControl.Model.Cards
{
    /// <summary>
    /// 角色卡片对象
    /// </summary>
    class PlayerCard : CharacterCard
    {
        public int cardOwnerId;
        public int cardLevel;
        public int specialHealth;
        public int specialEnergy;
        public int specialAttack;
        public int specialSpeed;
        public int currentHealth;
        public int currentEnergy;
        public int maxHealth;
        public int maxEnergy;

        protected List<Skill> cardOwnSkill = new List<Skill>();
        protected List<StateSkill> PlayerCardState = new List<StateSkill>();

        #region 属性获取
        public int GetHealth()
        {
            int value = this.baseHealth + this.growHealth * this.cardLevel + this.specialHealth;
            return value;
        }
        public int GetEnergy()
        {
            int value = this.baseEnergy + this.growEnergy * this.cardLevel + this.specialEnergy;
            return value;
        }
        public int GetAttack()
        {
            int value = this.baseAttack + this.growAttack * this.cardLevel + this.specialAttack;
            return value;
        }
        public int GetSpeed()
        {
            int value = this.baseSpeed + this.growSpeed * this.cardLevel + this.specialSpeed;
            return value;
        }

        public List<Skill> GetCardOwnSkill()
        { return this.cardOwnSkill; }
        #endregion

        #region 属性设置
        public void AddSkill(Skill addedSkill)
        {
            int skillID = addedSkill.skillID;
            //检查重复性
            foreach (Skill skill in cardOwnSkill)
            {
                if (skill.skillID == skillID)
                {
                    LogsSystem.Instance.Print("添加失败，该玩家已经有了同样ID的技能" + skillID, LogLevel.WARN);
                    return;
                }
            }

            this.cardOwnSkill.Add(addedSkill);
        }
        public void SetCardOwnSkill(List<Skill> skillList)
        {
            this.cardOwnSkill = skillList;
        }
        #endregion

        public void SetPlayerCardInfo(int cardOwnerId, int cardLevel, int specialHealth, int specialEnergy, int specialAttack, int specialSpeed)
        {
            this.cardOwnerId = cardOwnerId;
            this.cardLevel = cardLevel;
            this.specialHealth = specialHealth;
            this.specialEnergy = specialEnergy;
            this.specialAttack = specialAttack;
            this.specialSpeed = specialSpeed;

            this.maxHealth = this.currentHealth = this.GetHealth();
            this.maxEnergy = this.currentEnergy = this.GetEnergy();
        }

        /// <summary>
        /// 将血量和能量初始化为最开始值
        /// </summary>
        public void InitHealthAndEnergy()
        {
            this.maxHealth = this.currentHealth = this.GetHealth();
            this.maxEnergy = this.currentEnergy = this.GetEnergy();
        }

        public CardInfo GetCardInfo()
        {
            CardInfo info = new CardInfo();
            info.cardUUID = this.cardUUID;
            info.cardId = this.cardId;
            info.cardName = this.cardName;
            info.cardOwnerId = this.cardOwnerId;
            info.cardRarity = this.cardRarity;
            info.cardLevel = this.cardLevel;
            info.cardOwnSkill = IntArray.IntArrayToString(GetSkillIdArray());
            info.health = this.GetHealth();
            info.energy = this.GetSpeed();
            info.attack = this.GetAttack();
            info.speed = this.GetSpeed();

            return info;
        }

        /// <summary>
        /// 获得伤害
        /// </summary>
        public void GetDamage(int damageValue)
        {
            this.currentHealth -= damageValue;
            LogsSystem.Instance.Print(string.Format("角色{0}受到了{1}点伤害", this.cardName, damageValue));
        }

        /// <summary>
        /// 附加状态
        /// </summary>
        public void AppendState(StateSkill state)
        {
            this.PlayerCardState.Add(state);
            LogsSystem.Instance.Print(string.Format("卡片{0}获得了状态{1}", this.cardName, state.skillName), LogLevel.GAMEDETAIL);
        }

        /// <summary>
        /// 根据技能ID获取角色技能
        /// </summary>
        public Skill GetSkillById(int skillID)
        {
            foreach (Skill skill in cardOwnSkill)
            {
                if (skill.skillID == skillID)
                {
                    return skill;
                }
            }

            LogsSystem.Instance.Print("该角色没有这个技能" + skillID, LogLevel.WARN);
            return null;
        }

        /// <summary>
        /// 获取卡片拥有的技能ID列表
        /// 用于传递数据
        /// </summary>
        public int[] GetSkillIdArray()
        {
            List<int> skillIdList = new List<int>();
            foreach (Skill skill in cardOwnSkill)
            {
                skillIdList.Add(skill.skillID);
            }

            return skillIdList.ToArray();
        }


        /// <summary>
        /// 根据卡片ID创建卡片数据
        /// </summary>        
        private static PlayerCard CreatePlayerCardByID(int cardID)
        {
            CharacterCard character = CardManager.Instance.GetCardCloneByID(cardID);
            PlayerCard playerCard = new PlayerCard();
            playerCard.cardUUID = character.cardUUID;
            playerCard.cardId = character.cardId;
            playerCard.cardName = character.cardName;
            playerCard.cardRarity = character.cardRarity;
            playerCard.baseHealth = character.baseHealth;
            playerCard.baseEnergy = character.baseEnergy;
            playerCard.baseAttack = character.baseAttack;
            playerCard.baseSpeed = character.baseSpeed;
            playerCard.growHealth = character.growHealth;
            playerCard.growEnergy = character.growEnergy;
            playerCard.growAttack = character.growAttack;
            playerCard.growSpeed = character.growSpeed;

            return playerCard;
        }
        public static PlayerCard CreatePlayerCardByID(int cardID, int cardOwnerId, int cardLevel, int specialHealth, int specialEnergy, int specialAttack, int specialSpeed)
        {
            PlayerCard playerCard = CreatePlayerCardByID(cardID);
            playerCard.SetPlayerCardInfo(cardOwnerId, cardLevel, specialHealth, specialEnergy, specialAttack, specialSpeed);

            return playerCard;
        }
    }
}
