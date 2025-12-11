using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace XUtils.CollectionsUtils
{
    public static class XCollections
    {
#nullable enable
        /// <summary>
        /// Returns the element at the specified index or the default value if the index is out of range.
        /// </summary>
        public static T? GetAt<T>(this IEnumerable<T> source, int index)
        {
            foreach ((int i, T item) in source.Index())
                if (i == index)
                    return item;

            return default;
        }

        /// <summary>
        /// Determines whether all elements in the sequence satisfy the specified predicate.
        /// </summary>
        public static bool AllMatch<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T item in source)
                if (!predicate(item))
                    return false;

            return true;
        }

        /// <summary>
        /// Lazily generates a sequence using the selector for indices from 0 to count - 1.
        /// </summary>
        public static IEnumerable<T> Enumerate<T>(Func<int, T> selector, int count)
        {
            for (int i = 0; i < count; i++)
                yield return selector(i);
        }

        /// <summary>
        /// Searches in the dictionary for the first key whose value equals the specified target value.
        /// Returns the key or the default value if none is found.
        /// </summary>
        public static TKey? GetKeyFromValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue targetValue)
        {
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
                if (pair.Value?.Equals(targetValue) ?? false)
                    return pair.Key;

            return default;
        }

        /// <summary>
        /// Returns the first element in the sequence that satisfies the predicate, or the default value if none is found.
        /// </summary>
        public static T? Find<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (T item in source)
                if (predicate(item))
                    return item;

            return default;
        }
#nullable disable
        /// <summary>
        /// Finds an element in the list using the given predicate and returns whether it was found.
        /// The found element is returned via the out parameter.
        /// </summary>
        public static bool TryFind<T>(this List<T> list, Predicate<T> match, out T result)
        {
            int index = list.FindIndex(match);
            if (index >= 0)
            {
                result = list[index];
                return true;
            }

            result = default;
            return false;
        }
#nullable enable
        /// <summary>
        /// Returns the first element that satisfies the predicate together with its index. If none is found, returns (0, default).
        /// </summary>
        public static (int index, T? item) FindWithIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach ((int Index, T Item) entry in source.Index())
                if (predicate(entry.Item))
                    return entry;

            return (0, default);
        }

        /// <summary>
        /// Returns the index of the first element that satisfies the predicate, or -1 if none is found.
        /// </summary>
        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach ((int Index, T Item) entry in source.Index())
                if (predicate(entry.Item))
                    return entry.Index;

            return -1;
        }

        /// <summary>
        /// Concatenates all elements into a single string, separated by the given separator.
        /// </summary>
        public static string ToSeparatedString(this IEnumerable<object> source, string separator = ", ")
        {
            string outStr = string.Empty;

            foreach ((int index, object item) in source.Index())
                outStr += $"{(index > 0 ? separator : string.Empty)}{item}";

            return outStr;
        }

        /// <summary>
        /// Projects each element of the source sequence into a new form using the provided converter.
        /// </summary>
        public static IEnumerable<TOut> ConvertTo<TSource, TOut>(this IEnumerable<TSource> source, Converter<TSource, TOut> converter)
        {
            foreach (TSource item in source)
                yield return converter(item);
        }

        /// <summary>
        /// Applies the selector to each element and returns the resulting sequence.
        /// </summary>
        public static IEnumerable<T> SelectEach<T>(this IEnumerable<T> source, Func<T, T> selector)
        {
            foreach (T item in source)
                yield return selector(item);
        }

        /// <summary>
        /// Returns a random element from the sequence, or the default value if the sequence is empty.
        /// </summary>
        public static T? RandomElement<T>(this IEnumerable<T> source, Random? random = null)
        {
            int count = source.Count();
            if (count <= 0)
                return default;

            Random rnd = random ?? new Random();
            int index = rnd.Next(0, count);

            foreach ((int i, T item) in source.Index())
                if (i == index)
                    return item;

            return default;
        }

        /// <summary>
        /// Enumerates the source with indices, returning a sequence of (Index, Item) tuples.
        /// </summary>
        public static IEnumerable<(int Index, TSource Item)> Index<TSource>(this IEnumerable<TSource> source)
        {
            int index = -1;
            foreach (TSource element in source)
            {
                index++;
                yield return (index, element);
            }
        }

        /// <summary>
        /// Tries to get the count of the sequence without forcing full enumeration when possible.
        /// </summary>
        public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
        {
            if (source is ICollection<T> genericCollection)
            {
                count = genericCollection.Count;
                return true;
            }

            if (source is System.Collections.ICollection nonGenericCollection)
            {
                count = nonGenericCollection.Count;
                return true;
            }

            if (source is IReadOnlyCollection<T> readOnlyCollection)
            {
                count = readOnlyCollection.Count;
                return true;
            }

            count = 0;
            return false;
        }

        /// <summary>
        /// Splits the source into chunks of the specified size.
        /// </summary>
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            using IEnumerator<T> e = source.GetEnumerator();
            while (true)
            {
                T[] buffer = new T[size];
                int index = 0;

                while (index < size && e.MoveNext())
                {
                    buffer[index++] = e.Current;
                }

                if (index == 0)
                    yield break;

                if (index < size)
                {
                    Array.Resize(ref buffer, index);
                }

                yield return buffer;
            }
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using a key selector.
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            comparer ??= EqualityComparer<TKey>.Default;
            HashSet<TKey> set = new HashSet<TKey>(comparer);

            foreach (TSource element in source)
            {
                TKey key = keySelector(element);
                if (set.Add(key))
                    yield return element;
            }
        }

        /// <summary>
        /// Returns the maximum element of a sequence, based on a key.
        /// </summary>
        public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            comparer ??= Comparer<TKey>.Default;

            using IEnumerator<TSource> e = source.GetEnumerator();
            if (!e.MoveNext())
                return default;

            TSource max = e.Current;
            TKey maxKey = keySelector(max);

            while (e.MoveNext())
            {
                TSource candidate = e.Current;
                TKey candidateKey = keySelector(candidate);

                if (comparer.Compare(candidateKey, maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateKey;
                }
            }

            return max;
        }

        /// <summary>
        /// Returns the minimum element of a sequence, based on a key.
        /// </summary>
        public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            comparer ??= Comparer<TKey>.Default;

            using IEnumerator<TSource> e = source.GetEnumerator();
            if (!e.MoveNext())
                return default;

            TSource min = e.Current;
            TKey minKey = keySelector(min);

            while (e.MoveNext())
            {
                TSource candidate = e.Current;
                TKey candidateKey = keySelector(candidate);

                if (comparer.Compare(candidateKey, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateKey;
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the index of the first element that matches the predicate, or -1 if none.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition. Equivalent to Any(predicate) but as an extension name that reads well on Unity's older API surface.
        /// </summary>
        public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (T item in source)
                if (predicate(item))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines whether the LayerMask contains the specified layer.
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer) => (mask & (1 << layer)) != 0;

        /// <summary>
        /// Returns a valid index by wrapping around the given length.
        /// </summary>
        public static int WrapIndex(int index, int length) => (index % length + length) % length;
#nullable disable
    }
}