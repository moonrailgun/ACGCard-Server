using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardServerControl.Model;
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

        public CardManager()
        {
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

        /// <summary>
        /// 根据ID获取卡片的克隆
        /// </summary>
        public CharacterCard GetCardCloneByID(int cardId)
        {
            return characterCardMap[cardId].Clone() as CharacterCard;
        }
    }
}
