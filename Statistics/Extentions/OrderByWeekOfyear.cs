using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Statistics.Extentions
{
    public static class OrderByWeekOfYear
    {
        public static List<KeyValuePair<int, int>> ToOrderByWeekOfYear(this object obj)
        {
            if (obj == null)
                return new List<KeyValuePair<int, int>>();
            var source = obj as Dictionary<int, int>;
            if (source == null)
                return null;
            var key = 1;
            var dictionary = new Dictionary<int, int>();
            foreach (var keyValuePair in source)
            {
                for (; keyValuePair.Key != key; ++key)
                    dictionary.Add(key, 0);
                ++key;
            }
            foreach (var keyValuePair in dictionary)
                source.Add(keyValuePair.Key, keyValuePair.Value);
            var keyValuePairList = new List<KeyValuePair<int, int>>();
            var weekNow = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            keyValuePairList.AddRange(source.Where(k => k.Key <= weekNow).OrderByDescending(k => k.Key).Select(item => new KeyValuePair<int, int>(item.Key, item.Value)));
            keyValuePairList.AddRange(source.Where(k => k.Key > weekNow).OrderByDescending(k => k.Key).Select(item => new KeyValuePair<int, int>(item.Key, item.Value)));
            return keyValuePairList;
        }
    }
}
