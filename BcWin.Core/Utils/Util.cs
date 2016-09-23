using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Core.Utils
{
    public static class Util
    {
        //public static void Shuffle<T>(this IList<T> list)
        //{
        //    Random rng = new Random();
        //    int n = list.Count;
        //    while (n > 1)
        //    {
        //        n--;
        //        int k = rng.Next(n + 1);
        //        T value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}

        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IDictionary<TKey, TValue> Shuffle<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            Random r = new Random();
            return source.OrderBy(x => r.Next()).ToDictionary(item => item.Key, item => item.Value);
        }

        public static IEnumerable<KeyValuePair<TKey, TValue>> RandomValues<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, int take)
        {
            int size = dict.Count;
            if (size > take)
            {
                Random rand = new Random();
                List<KeyValuePair<TKey, TValue>> values = new List<KeyValuePair<TKey, TValue>>();
                for (int i = 0; i < take; i++)
                {
                    values.Add( dict.ElementAt(rand.Next(size)));
                    //values.Add(aa);
                }
                return values;
            }
            return dict.ToList();
        }

        public static int Round(this int i, int nearest)
        {
            if (nearest <= 0 || nearest % 10 != 0)
                throw new ArgumentOutOfRangeException("nearest", "Must round to a positive multiple of 10");

            return (i + 5 * nearest / 10) / nearest * nearest;
        }
    }
}
