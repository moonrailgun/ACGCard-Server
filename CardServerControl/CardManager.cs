using System;
using System.Collections.Generic;
using System.Data;
using CardServerControl.Model.Cards;

namespace CardServerControl
{
    /// <summary>
    /// 卡片基础通用信息管理类
    /// 数据来源于数据库
    /// </summary>
    class CardManager
    {
        #region 单例模式
        private static CardManager _instance;
        public static CardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardManager();
                }
                return _instance;
            }
        }
        #endregion

        public Dictionary<int, CharacterCard> characterCardMap;
        public Dictionary<int, ItemCard> itemCardMap;

        #region 数据库初始化
        public CardManager()
        {
            InitCharacterCardMap();
            InitItemCardMap();
        }
        /// <summary>
        /// 初始化角色卡
        /// </summary>
        private void InitCharacterCardMap()
        {
            LogsSystem.Instance.Print("正在从数据库获取英雄卡数据");
            try
            {
                characterCardMap = new Dictionary<int, CharacterCard>();

                string command = "SELECT * FROM card ORDER BY CardId";
                DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    try
                    {
                        //从数据库中取值
                        CharacterCard characterCard = new CharacterCard();
                        characterCard.cardId = Convert.ToInt32(row["CardId"]);
                        characterCard.cardName = row["CardName"].ToString();
                        characterCard.cardRarity = Convert.ToInt32(row["CardRarity"]);
                        characterCard.baseHealth = Convert.ToInt32(row["BaseHealth"]);
                        characterCard.baseEnergy = Convert.ToInt32(row["BaseEnergy"]);
                        characterCard.baseAttack = Convert.ToInt32(row["BaseAttack"]);
                        characterCard.baseSpeed = Convert.ToInt32(row["BaseSpeed"]);
                        characterCard.growHealth = Convert.ToInt32(row["GrowHealth"]);
                        characterCard.growEnergy = Convert.ToInt32(row["GrowEnergy"]);
                        characterCard.growAttack = Convert.ToInt32(row["GrowAttack"]);
                        characterCard.growSpeed = Convert.ToInt32(row["GrowSpeed"]);

                        characterCardMap.Add(characterCard.cardId, characterCard);
                    }
                    catch (Exception ex)
                    {
                        LogsSystem.Instance.Print(ex.ToString(), LogLevel.WARN);
                    }
                }

                LogsSystem.Instance.Print("卡片数据加载完毕");
            }
            catch (Exception ex)
            {
                LogsSystem.Instance.Print("卡片数据加载失败：" + ex, LogLevel.ERROR);
            }
        }

        /// <summary>
        /// 初始化道具卡
        /// </summary>
        private void InitItemCardMap()
        {
            LogsSystem.Instance.Print("正在从数据库获取道具卡数据");
            throw new NotImplementedException();
        }
        #endregion


        #region 角色卡操作
        public int GetRarityByCardId(int cardId)
        {
            if (characterCardMap.ContainsKey(cardId))
            {
                CharacterCard character = characterCardMap[cardId];
                return character.cardRarity;
            }
            else
            {
                return 0;
            }
        }

        public string GetNameById(int cardId)
        {
            if (characterCardMap.ContainsKey(cardId))
            {
                CharacterCard character = characterCardMap[cardId];
                return character.cardName;
            }
            else
            {
                return "";
            }
        }
        #endregion

        /// <summary>
        /// 根据ID获取卡片的克隆
        /// </summary>
        public CharacterCard GetCharacterCloneByID(int cardId)
        {
            if (characterCardMap.ContainsKey(cardId))
            {
                return characterCardMap[cardId].Clone() as CharacterCard;
            }
            else
            {
                LogsSystem.Instance.Print("[卡片管理器]无法找到该ID的卡片：" + cardId);
                return null;
            }
        }
        public ItemCard GetItemCloneByID(int cardId)
        {
            if (itemCardMap.ContainsKey(cardId))
            {
                return itemCardMap[cardId].Clone() as ItemCard;
            }
            else
            {
                LogsSystem.Instance.Print("[卡片管理器]无法找到该ID的卡片：" + cardId);
                return null;
            }
        }
    }
}
