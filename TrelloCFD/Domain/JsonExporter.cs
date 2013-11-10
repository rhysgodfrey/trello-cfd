using Chello.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrelloCFD.Models;

namespace TrelloCFD.Domain
{
    public class JsonExporter : TrelloApiBase
    {
        private readonly string _boardId;

        public JsonExporter(string boardId, string apiKey, string authToken)
            : base(apiKey, authToken)
        {
            _boardId = boardId;
        }

        public string BuildCsv()
        {
            var json = GetRequest("/board/{0}?cards=all&lists=all&memberships=all&members=all", _boardId);

            var cards = ExtractTrelloCardsFromJson(json);

            return BuildCsv(cards);
        }

        private static string BuildCsv(TrelloCard[] cards)
        {
            if (cards != null)
            {
                StringBuilder builder = new StringBuilder();

                string _rowDelimiter = "\n";

                builder.Append("Name,Section,Labels");

                builder.Append(_rowDelimiter);

                foreach (TrelloCard card in cards)
                {
                    builder.Append(card.Name);
                    builder.Append(",");
                    builder.Append(card.List);
                    builder.Append(",");
                    builder.Append(card.Label);
                    builder.Append(_rowDelimiter);
                }

                return builder.ToString();
            }

            return "";
        }

        private static TrelloCard[] ExtractTrelloCardsFromJson(string jsonText)
        {
            JProperty cards;
            JProperty lists;

            try
            {
                JObject jsonObject = JObject.Parse(jsonText);
                cards = jsonObject.Properties().FirstOrDefault(a => String.Equals(a.Name, "cards", StringComparison.OrdinalIgnoreCase));
                lists = jsonObject.Properties().FirstOrDefault(a => String.Equals(a.Name, "lists", StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }

            if (cards != null && lists != null)
            {
                JArray cardDetails = cards.Children().FirstOrDefault() as JArray;

                JArray listDetails = lists.Children().FirstOrDefault() as JArray;

                if (cardDetails != null && listDetails != null)
                {
                    return BuildTrelloCardList(cardDetails, listDetails).ToArray();
                }
            }

            return null;
        }

        private static string CleanToken(JToken token)
        {
            return token.ToString().Replace(",", "");
        }

        private static IEnumerable<TrelloCard> BuildTrelloCardList(JArray cardDetails, JArray listDetails)
        {
            IDictionary<string, string> lists = new Dictionary<string, string>();

            foreach (JToken token in listDetails)
            {
                lists.Add(CleanToken(token["id"]), CleanToken(token["name"]));
            }

            foreach (JToken token in cardDetails)
            {
                yield return new TrelloCard()
                {
                    Name = ExtractName(token),
                    Label = ExtractLabelName(token),
                    List = ExtractListName(token, lists)
                };
            }
        }

        private static string ExtractName(JToken token)
        {
            return CleanToken(token["name"]);
        }

        private static string ExtractListName(JToken token, IDictionary<string, string> lists)
        {
            string listId = token["idList"].ToString();

            return lists.Where(o => o.Key == listId).FirstOrDefault().Value;
        }

        private static string ExtractLabelName(JToken token)
        {
            StringBuilder builder = new StringBuilder();

            JToken labels = token["labels"];

            if (labels != null)
            {
                var labelsWithNames = labels.Children().Where(a => a["name"] != null);

                foreach (var label in labelsWithNames)
                {
                    builder.Append(CleanToken(label["name"]));
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }
    }
}
