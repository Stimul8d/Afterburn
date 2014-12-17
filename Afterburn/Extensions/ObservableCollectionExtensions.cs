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
    }
}