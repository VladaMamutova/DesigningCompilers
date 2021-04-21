using System;

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
    }
}
