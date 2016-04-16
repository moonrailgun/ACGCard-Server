using CardServerControl.Model.Skills.ContinuedSkill;
using CardServerControl.Model.Skills;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardServerControl.Util;
using CardServerControl.Model.DTO.GameData;
using CardServerControl.Model.Cards.EquipmentCards;

namespace CardServerControl.Model.Cards
{
    /// <summary>
    /// 角色卡片对象
    /// </summary>
    class PlayerCard : CharacterCard
    {
        public PlayerCard()
        {
            this.cardOwnSkill = new List<Skill>();
            this.playerCardState = new List<StateSkill>();
            this.playerEquipment = new PlayerEquipment(this);
        }

        public bool isAlive = true;//卡片是否存活

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

        protected bool isAvailable = true;//卡片是否可用

        protected GameRoom cardRoom;
        protected int cardOwnerPostion = -1;
        protected List<Skill> cardOwnSkill;
        protected List<StateSkill> playerCardState;//卡片上附加的状态

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

            foreach (StateSkill skill in playerCardState)
            {
                if (skill is AttackUp)
                {
                    AttackUp attackUpSkill = skill as AttackUp;
                    value += attackUpSkill.GetAddedValue();
                }
            }

            return value;
        }
        public int GetSpeed()
        {
            int value = this.baseSpeed + this.growSpeed * this.cardLevel + this.specialSpeed;
            return value;
        }

        public List<Skill> GetCardOwnSkill()
        { return this.cardOwnSkill; }

        public int GetCardOwnerPosition()
        { return this.cardOwnerPostion; }
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
        public void SetOwnerData(GameRoom cardRoom, int position)
        {
            this.cardRoom = cardRoom;
            this.cardOwnerPostion = position;
        }
        #endregion

        #region 技能状态操作
        public void AddState(StateSkill skill)
        {
            skill.SetStateInfo(this);
            this.playerCardState.Add(skill);
            LogsSystem.Instance.Print(string.Format("英雄 {0} 获得了状态 {1} ，持续 {2} 回合", this.cardName, skill.skillName, skill.GetRemainRounds()), LogLevel.GAMEDETAIL);

            //通过技能的USE操作添加
            //TcpServer.Instance.GetTCPDataSender().SendStateOperate(skill, this, OperateStateData.StateOperateCode.AddState, cardOwnerPostion, cardRoom);
        }

        /// <summary>
        /// 移除状态
        /// </summary>
        public void RemoveState(StateSkill skill)
        {
            if (playerCardState.Contains(skill))
            {
                this.playerCardState.Remove(skill);
                LogsSystem.Instance.Print(string.Format("英雄 {0} 移除了状态 {1}", this.cardName, skill.skillName), LogLevel.GAMEDETAIL);

                //向服务器请求去除状态
                TcpServer.Instance.GetTCPDataSender().SendStateOperate(skill, this, OperateStateData.StateOperateCode.RemoveState, cardOwnerPostion, cardRoom);
            }
            else
            {
                LogsSystem.Instance.Print("卡片上不具备该技能：" + skill.skillName, LogLevel.WARN);
            }
        }
        #endregion

        #region 装备操作
        public PlayerEquipment playerEquipment;

        public class PlayerEquipment
        {
            public PlayerEquipment(PlayerCard playerCard)
            { this.playerCard = playerCard; }

            public PlayerCard playerCard;

            public EquipmentCard weapon;//武器
            public EquipmentCard armor;//盔甲
            public EquipmentCard jewelry1;//首饰1
            public EquipmentCard jewelry2;//首饰2

            /// <summary>
            /// 装备装备卡
            /// </summary>
            public void Equip(int postion, EquipmentCard equip)
            {
                this.Equip((Position)postion, equip);
            }
            public void Equip(Position position, EquipmentCard equip)
            {
                switch (position)
                {
                    case Position.Weapon:
                        {
                            if (this.weapon == null)
                            {
                                //角色没有装备
                                this.weapon = equip;
                                equip.OnEquiped(playerCard);
                            }
                            else
                            {
                                //角色已经有装备了
                                this.weapon.OnUnequiped();
                                this.weapon = equip;
                                equip.OnEquiped(playerCard);
                            }
                            break;
                        }
                    case Position.Armor:
                        {
                            if (this.armor == null)
                            {
                                //角色没有装备
                                this.armor = equip;
                                equip.OnEquiped(playerCard);
                            }
                            else
                            {
                                //角色已经有装备了
                                this.armor.OnUnequiped();
                                this.armor = equip;
                                equip.OnEquiped(playerCard);
                            }
                            break;
                        }
                    case Position.Jewelry:
                        {
                            EquipmentCard jewelry = equip;
                            equip.OnEquiped(playerCard);
                            //如果两个首饰槽都有装备了。覆盖稀有度小的。稀有度一样则随机覆盖
                            if (this.jewelry1 != null && this.jewelry2 != null)
                            {
                                if (this.jewelry1.cardRarity != this.jewelry2.cardRarity)
                                {
                                    //替换稀有度低的
                                    if (this.jewelry1.cardRarity > this.jewelry2.cardRarity)
                                    {
                                        this.jewelry1 = jewelry;
                                    }
                                    else
                                    {
                                        this.jewelry2 = jewelry;
                                    }
                                }
                                else
                                {
                                    //随机替换
                                    System.Random random = new System.Random();
                                    float randVal = (float)random.Next(0, 1000) / 1000;

                                    if (randVal < 0.5f)
                                    {
                                        this.jewelry1 = jewelry;
                                    }
                                    else
                                    {
                                        this.jewelry2 = jewelry;
                                    }
                                }
                            }
                            else
                            {
                                if (this.jewelry1 == null)
                                {
                                    this.jewelry1 = jewelry;
                                }
                                else if (this.jewelry2 == null)
                                {
                                    this.jewelry2 = jewelry;
                                }
                            }
                            break;
                        }
                    default:
                        {
                            LogsSystem.Instance.Print("不合法的装备位置");
                            break;
                        }
                }
            }

            public enum Position
            {
                Weapon = 1, Armor = 2, Jewelry = 3
            }
        }


        #endregion

        #region 卡片可用性
        public bool GetAvailable(){
            return this.isAvailable;
        }
        public void SetAvailable(bool available) {
            this.isAvailable = available;
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

            if (this.currentHealth <= 0)
            {
                OnDead();
            }
        }

        /// <summary>
        /// 角色死亡
        /// </summary>
        private void OnDead()
        {
            this.isAlive = false;
        }

        /// <summary>
        /// 附加状态
        /// </summary>
        public void AppendState(StateSkill state)
        {
            this.playerCardState.Add(state);
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
            CharacterCard character = CardManager.Instance.GetCharacterCloneByID(cardID);
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
