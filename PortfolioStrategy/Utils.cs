using System;

namespace PortfolioStrategy
{
    static class Utils
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] AddToEnd<T>(this T[] data, T element)
        {
            T[] result = new T[data.Length + 1];
            data.CopyTo(result, 0);
            result[data.Length] = element;
            return result;
        }
    }
}
