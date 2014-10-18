using Chello.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrelloCFD.Factories;
using Chello.ScrumExtensions;

namespace TrelloCFD.Domain
{
    public class ActivityRepository
    {
        private string _boardId;
        private IDictionary<string, ActivityList> _lists;

        public ActivityRepository(string boardId)
        {
            _boardId = boardId;

            Initialize();
        }

        private void Initialize()
        {
            ChelloClient client = TrelloClientFactory.Get();

            _lists = client.Lists.ForBoard(_boardId).ToDictionary(l => l.Id, l => new ActivityList(l.Id, l.Name));

            IEnumerable<CardUpdateAction> updates =  client.CardUpdates.ForBoard(_boardId, new { limit = 1000, filter = "createCard,updateCard,moveCardFromBoard,moveCardToBoard,updateBoard,createBoard" });
            
            // HACK: Board closing doesn't come back properly - assume if last activity was a board update it is board closing
            // may give strange results for some cases
            if (updates.FirstOrDefault().Type.Equals("updateBoard", StringComparison.OrdinalIgnoreCase))
            {
                BoardClosed = DateTime.Parse(updates.FirstOrDefault().Date).ToUniversalTime();
            }

            ProcessUpdates(client, updates);
        }

        private void ProcessUpdates(ChelloClient client, IEnumerable<CardUpdateAction> updates)
        {
            if (updates == null)
            {
                return;
            }

            foreach (var update in updates.Reverse())
            {
                switch (update.Type.ToUpperInvariant())
                {
                    case "CREATECARD":
                    case "MOVECARDTOBOARD":
                        if (update.Data.List != null)
                        {
                            _lists.AddCard(update.Data.List.Id, new ActivityCard(update.Data.Card, DateTime.Parse(update.Date).ToUniversalTime()));
                        }
                        break;
                    case "UPDATECARD":
                        if (update.Data.ListAfter != null && update.Data.ListBefore != null)
                        {
                            _lists.FinishCard(update.Data.ListBefore.Id, update.Data.Card, DateTime.Parse(update.Date).ToUniversalTime());
                            _lists.AddCard(update.Data.ListAfter.Id, new ActivityCard(update.Data.Card, DateTime.Parse(update.Date).ToUniversalTime()));
                        }
                        else if (update.Data.Card.Closed)
                        {
                            foreach (string listId in _lists.Keys)
                            {
                                _lists.FinishCard(listId, update.Data.Card, DateTime.Parse(update.Date).ToUniversalTime());
                            }
                        }
                        break;
                    case "MOVECARDFROMBOARD":
                        // When a card is moved form a board, it's history isn't included in the boards activity any more, get it seperately
                        var cardHistory = client.CardUpdates.ForCard(update.Data.Card.Id, new { limit = 1000, filter = "createCard,updateCard" });
                        ProcessUpdates(client, cardHistory);
                        
                        foreach (string listId in _lists.Keys)
                        {
                            _lists.FinishCard(listId, update.Data.Card, DateTime.Parse(update.Date).ToUniversalTime());
                        }

                        break;
                    case "CREATEBOARD":
                        BoardOpened = DateTime.Parse(update.Date).ToUniversalTime();
                        LimitedByApi = false;
                        break;
                }

                if (LimitedByApi && updates.Any())
                {
                    BoardOpened = DateTime.Parse(updates.LastOrDefault().Date);
                }

                if (update.Data.Card != null)
                {
                    foreach (string listId in _lists.Keys)
                    {
                        _lists.UpdatePoints(listId, update.Data.Card);
                    }
                }
            }
        }

        public DateTime BoardOpened { get; private set; }
        public DateTime? BoardClosed { get; private set; }

        private bool? _limitedByApi = null;
        public bool LimitedByApi
        {
            get
            {
                if (_limitedByApi.HasValue)
                {
                    return _limitedByApi.Value;
                }

                return true;
            }
            set
            {
                _limitedByApi = value;
            }
        }

        public IEnumerable<ActivityList> Lists
        {
            get
            {
                return _lists.Values.ToArray();
            }
        }

        public class ActivityList
        {
            public ActivityList(string id, string name)
            {
                Id = id;
                Name = name;
            }

            public string Id { get; private set; }
            public string Name { get; private set; }

            private IDictionary<string, IList<ActivityCard>> _cards = new Dictionary<string, IList<ActivityCard>>();

            public IEnumerable<ActivityCard> Cards
            {
                get
                {
                    return _cards.Values.SelectMany(c => c).ToArray();
                }
            }

            public void AddCard(ActivityCard card)
            {
                if (_cards.ContainsKey(card.Id))
                {
                    _cards[card.Id].Add(card);
                }
                else
                {
                    IList<ActivityCard> newCards = new List<ActivityCard>();
                    newCards.Add(card);

                    _cards.Add(card.Id, newCards);
                }
            }

            public void FinishCard(string cardId, DateTime endDate)
            {
                if (_cards.Keys.Contains(cardId))
                {
                    foreach (var card in _cards[cardId])
                    {
                        if (!card.EndDate.HasValue)
                        {
                            card.EndCard(endDate);
                        }
                    }
                }
            }

            public void UpdatePoints(string cardId, int? updatedPoints)
            {
                if (_cards.Keys.Contains(cardId))
                {
                    foreach (var card in _cards[cardId])
                    {
                        card.UpdatePoints(updatedPoints);
                    }
                }
            }
        }

        public class ActivityCard
        {
            public ActivityCard(Card card, DateTime startDate)
                : this(card.Id, startDate, card.HasPoints() ? (int?)card.Points() : null)
            {
            }

            public ActivityCard(string id, DateTime startDate, int? points = null)
            {
                Id = id;
                StartDate = startDate;
                Points = points;
            }

            public string Id { get; private set; }
            public DateTime StartDate { get; private set; }
            public DateTime? EndDate { get; private set; }

            public int? Points { get; private set; }

            public void UpdatePoints(int? points)
            {
                Points = points;
            }

            public void EndCard(DateTime endDate)
            {
                EndDate = endDate;
            }
        }
    }

    public static class ActivityExtensions
    {
        public static void AddCard(this IDictionary<string, ActivityRepository.ActivityList> lists, string listId, ActivityRepository.ActivityCard card)
        {
            if (lists.ContainsKey(listId))
            {
                lists[listId].AddCard(card);
            }
        }

        public static void FinishCard(this IDictionary<string, ActivityRepository.ActivityList> lists, string listId, Card card, DateTime endDate)
        {
            if (lists.ContainsKey(listId))
            {
                lists[listId].FinishCard(card.Id, endDate);
            }
        }

        public static void UpdatePoints(this IDictionary<string, ActivityRepository.ActivityList> lists, string listId, Card card)
        {
            if (lists.ContainsKey(listId))
            {
                lists[listId].UpdatePoints(card.Id, card.HasPoints() ? (int?)card.Points() : null);
            }
        }
    }
}