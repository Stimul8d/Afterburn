using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Afterburn.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> me, IEnumerable<T> toAdd)
        {
            foreach (var item in toAdd)
            {
                me.Add(item);
            }
        }

        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            List<TSource> sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (var sortedItem in sortedList)
                source.Add(sortedItem);
        }
    }
}