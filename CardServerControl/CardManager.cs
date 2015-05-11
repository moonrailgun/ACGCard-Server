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
                    int cardId = Convert.ToInt32(row["CardId"]);
                    string cardName = row["CardName"].ToString();
                    int cardRarity = Convert.ToInt32(row["CardRarity"]);

                    Card card = new Card(cardId, cardName, cardRarity);
                    cardList.Add(card);
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
    }
}
