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
            Dictionary<int, int> source = obj as Dictionary<int, int>;
            if (source == null)
                return (List<KeyValuePair<int, int>>)null;
            int key = 1;
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> keyValuePair in source)
            {
                for (; keyValuePair.Key != key; ++key)
                    dictionary.Add(key, 0);
                ++key;
            }
            foreach (KeyValuePair<int, int> keyValuePair in dictionary)
                source.Add(keyValuePair.Key, keyValuePair.Value);
            List<KeyValuePair<int, int>> keyValuePairList = new List<KeyValuePair<int, int>>();
            int weekNow = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            keyValuePairList.AddRange(source.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(k => k.Key <= weekNow)).OrderByDescending<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(k => k.Key)).Select<KeyValuePair<int, int>, KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, KeyValuePair<int, int>>)(item => new KeyValuePair<int, int>(item.Key, item.Value))));
            keyValuePairList.AddRange(source.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>)(k => k.Key > weekNow)).OrderByDescending<KeyValuePair<int, int>, int>((Func<KeyValuePair<int, int>, int>)(k => k.Key)).Select<KeyValuePair<int, int>, KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, KeyValuePair<int, int>>)(item => new KeyValuePair<int, int>(item.Key, item.Value))));
            return keyValuePairList;
        }
    }
}
