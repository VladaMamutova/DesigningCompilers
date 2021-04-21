using System;
using System.Collections.Generic;
using System.Linq;

namespace GrammarTransformations.Extensions
{
    static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] data, int start, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, start, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index)
        {
            return data.SubArray(index, data.Length - index);
        }

        public static T[] SubArrayWithItems<T>(this T[] array, HashSet<T> items, int itemCount)
        {
            var subList = new List<T>();
            var count = 0;
            for (var i = 0; i < array.Length && count < itemCount; i++)
            {
                if (items.Contains(array[i]))
                {
                    subList.Add(array[i]);
                    count++;
                }
                else
                {
                    subList.Add(array[i]);
                }
            }

            return subList.ToArray();
        }

        public static T[] SubArrayWithoutItems<T>(this T[] array, HashSet<T> items, int start)
        {
            var subList = new List<T>();
            for (var i = start; i < array.Length; i++)
            {
                if (!items.Contains(array[i]))
                {
                    subList.Add(array[i]);
                }
            }

            return subList.ToArray();
        }

        public static List<string[]> GenerateCombinations(this string[] array,
            HashSet<string> items, int itemCount)
        {
            if (itemCount < 1)
            {
                return new List<string[]>();
            }

            if (itemCount == 1)
            {
                var combination = new List<string> { array[0] };
                combination.AddRange(array.SubArrayWithoutItems(items, 1));

                return new List<string[]> { combination.ToArray() };
            }

            var combinations = new List<string[]>();
            var combinationStart = array
                .SubArrayWithItems(items, itemCount - 1).ToList();
            for (var i = combinationStart.Count; i < array.Length; i++)
            {
                if (items.Contains(array[i]))
                {
                    var combination = new List<string>();
                    combination.AddRange(combinationStart);
                    combination.Add(array[i]);
                    combination.AddRange(
                        array.SubArrayWithoutItems(items, i + 1));
                    combinations.Add(combination.ToArray());
                }
                else
                {
                    combinationStart.Add(array[i]);
                }
            }

            combinations.AddRange(GenerateCombinations(array, items,
                itemCount - 1));

            return combinations;
        }

        public static string[] GetItemsByTotalLength(this string[] array, int length)
        {
            List<string> items = new List<string>();
            for (int i = 0; i < array.Length && length > 0; i++)
            {
                int itemLength = array[i].Length;
                if (length >= itemLength)
                {
                    items.Add(array[i]);
                    length -= itemLength;
                }
                else
                {
                    throw new ArgumentException(
                        $"Failed to get items. Item '{array[i]}' " +
                        "cannot be added to the list. The length is incorrect.",
                        nameof(length));
                }
            }

            return items.ToArray();
        }

        public static string FindPrefix(this string[] strings)
        {
            string maxPrefix = "";
            foreach (var str in strings)
            {
                var prefix = str;
                var found = false;

                while (prefix.Length > 0 && !found)
                {
                    var prefixCount = strings.Count(r => r.StartsWith(prefix));

                    if (prefixCount > 1)
                    {
                        found = true;
                        if (prefix.Length > maxPrefix.Length)
                        {
                            maxPrefix = prefix;
                        }
                    }
                    else
                    {
                        prefix = prefix.Remove(prefix.Length - 1);
                    }
                }
            }

            return maxPrefix;
        }
    }
}
