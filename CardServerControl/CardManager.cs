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
    class CardManager
    {
        public List<Card> cardList;

        public CardManager()
        {
            try
            {
                cardList = new List<Card>();

                string command = "SELECT * FROM card ORDER BY CardId";
                DataSet ds = MySQLHelper.GetDataSet(MySQLHelper.Conn, CommandType.Text, command, null);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    try
                    {
                        Card card = new Card();
                        card.cardId = Convert.ToInt32(row["CardId"]);
                        card.cardName = row["CardName"].ToString();
                        card.cardRarity = Convert.ToInt32(row["CardRarity"]);
                        card.baseHealth = Convert.ToInt32(row["BaseHealth"]);
                        card.baseEnergy = Convert.ToInt32(row["BaseEnergy"]);
                        card.baseAttack = Convert.ToInt32(row["BaseAttack"]);
                        card.baseSpeed = Convert.ToInt32(row["BaseSpeed"]);
                        card.growHealth = Convert.ToInt32(row["GrowHealth"]);
                        card.growEnergy = Convert.ToInt32(row["GrowEnergy"]);
                        card.growAttack = Convert.ToInt32(row["GrowAttack"]);
                        card.growSpeed = Convert.ToInt32(row["GrowSpeed"]);

                        cardList.Add(card);
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
            foreach (Card card in cardList)
            {
                if (card.cardId == cardId)
                {
                    return card.cardRarity;
                }
            }
            return 0;
        }

        public string GetNameById(int cardId)
        {
            foreach (Card card in cardList)
            {
                if (card.cardId == cardId)
                {
                    return card.cardName;
                }
            }
            return "";
        }

        public Card GetCardByID(int cardId)
        {
            foreach (Card card in cardList)
            {
                if (card.cardId == cardId)
                {
                    return card.Clone() as Card;
                }
            }
            return null;
        }
    }
}
