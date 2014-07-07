using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace WorkItemFinder
{
    public class Domäne
    {
        private BlockingCollection<SearchWorkItem> _cachedItems = null;

        private Action _cachingFinished;

        public Domäne(Action cachingFinished)
        {
            _cachingFinished = cachingFinished;
            _cachedItems = new BlockingCollection<SearchWorkItem>();
            TfsAnbindung.GetItemsAsync((item) => _cachedItems.Add(item), _cachingFinished);
        }

        public int CacheSize
        {
            get
            {
                return _cachedItems.Count;
            }
        }

        public List<SearchWorkItem> FilterItems(string filterExpression)
        {
            List<SearchWorkItem> result = null;
            filterExpression = filterExpression.ToLower();
            if (FilterExpressionIsValid(filterExpression))
            {
                var found = from searchWorkItem in _cachedItems
                            where searchWorkItem.SearchStringShort.Contains(filterExpression)
                            orderby searchWorkItem.Id descending
                            select searchWorkItem;
                if (found.Any())
                {
                    result = found.ToList();
                }
            }
            return result;
        }

        private bool FilterExpressionIsValid(string filterExpression)
        {
            var retVal = false;

            var longEnough = filterExpression.Length > 2;
            var notNullOrWhiteSpace = !string.IsNullOrWhiteSpace(filterExpression);

            retVal = longEnough && notNullOrWhiteSpace;

            return retVal;
        }

        public void RefreshCache()
        {
            Task.Factory.StartNew(() =>
            {
                var newCache = new BlockingCollection<SearchWorkItem>();
                TfsAnbindung.GetItems(newCache.Add);
                _cachedItems = newCache;
                if (_cachingFinished != null)
                {
                    _cachingFinished();
                }
            });
        }
    }
}
