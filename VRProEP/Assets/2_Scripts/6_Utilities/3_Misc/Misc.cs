using System;
using System.Collections.Generic;

namespace VRProEP.Utilities
{
    public static class Misc
    {
        private static System.Random rng = new System.Random();

        /// <summary>
        /// Shuffles the contents in a list using the Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The list type.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
