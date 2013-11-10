using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrelloCFD.Domain;

namespace TrelloCFD.Models
{
    public class CumulativeFlowModel
    {
        private ActivityRepository _repository;
        private DateTime _beginTime;
        private DateTime _endTime;
        private DateTime _displayBeginTime;
        private DateTime _displayEndTime;
        private Func<IEnumerable<ActivityRepository.ActivityCard>, int> _calculation;
        private string _title;
        private bool _limitedByTrello;

        public CumulativeFlowModel(ActivityRepository repository)
            : this(repository, DateTime.MinValue, DateTime.MaxValue)
        {
        }

        public CumulativeFlowModel(ActivityRepository repository, Func<IEnumerable<ActivityRepository.ActivityCard>, int> calculation, string title)
            : this(repository, DateTime.MinValue, DateTime.MaxValue, calculation, title)
        {
        }

        public CumulativeFlowModel(ActivityRepository repository, DateTime displayStartDate, DateTime displayEndDate)
            : this(repository, displayStartDate, displayEndDate, (cards) => { return cards.Count(); }, "Cards")
        {
        }

        public CumulativeFlowModel(ActivityRepository repository, DateTime displayStartDate, DateTime displayEndDate, Func<IEnumerable<ActivityRepository.ActivityCard>, int> calculation, string title)
        {
            _repository = repository;
            _beginTime = repository.BoardOpened;
            _endTime = repository.BoardClosed.HasValue ? repository.BoardClosed.Value : DateTime.UtcNow;
            _calculation = calculation;
            _title = title;
            _limitedByTrello = repository.LimitedByApi;
            _displayBeginTime = displayStartDate < _beginTime ? _beginTime : displayStartDate;
            _displayEndTime = displayEndDate > _endTime ? _endTime : displayEndDate;
        }

        public string Title
        {
            get { return _title; }
        }

        public bool LimitedByTrello
        {
            get { return _limitedByTrello; }
        }

        public DateTime StartDate
        {
            get { return _beginTime; }
        }

        public DateTime EndDate
        {
            get { return _endTime; }
        }

        public DateTime DisplayStartDate
        {
            get { return _displayBeginTime; }
        }

        public DateTime DisplayEndDate
        {
            get { return _displayEndTime; }
        }

        public IEnumerable<DateTime> Periods
        {
            get
            {
                TimeSpan span = _displayEndTime - _displayBeginTime;
                double minutesPerPeriod = span.TotalMinutes / 15;

                DateTime current = _displayBeginTime;
                
                while (current < _displayEndTime)
                {
                    yield return current;

                    current = current.AddMinutes(minutesPerPeriod);
                }

                yield return _displayEndTime;
            }
        }

        public IEnumerable<ListData> Lists
        {
            get
            {
                DateTime[] periods = Periods.ToArray();

                foreach (var list in _repository.Lists)
                {
                    IList<int> counts = new List<int>();

                    for (int i = 0; i < periods.Count(); i++)
                    {
                        int count = _calculation(list.Cards.Where(c => periods[i] >= c.StartDate && c.EndDate.HasValue && periods[i] < c.EndDate.Value));
                        count += _calculation(list.Cards.Where(c => periods[i] >= c.StartDate && !c.EndDate.HasValue));
                    
                        counts.Add(count);
                    }

                    yield return new ListData(list.Name, counts);
                }
            }
        }

        public class ListData
        {
            public ListData(string name, IEnumerable<int> counts)
            {
                Name = name;
                Counts = counts;
            }

            public string Name { get; private set; }
            public IEnumerable<int> Counts { get; private set; }
        }
    }
}