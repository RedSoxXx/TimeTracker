using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static int CalculateHash<T>(this IEnumerable<T> self, int? seed = null)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (!self.Any() && !seed.HasValue)
                throw new ArgumentException($"{nameof(seed)} can't be null when {nameof(self)} is empty.");

            const int hashMultiplier = 397;
            return self.Aggregate(seed ?? 17, (h, i) => unchecked((h * hashMultiplier) ^ i.GetHashCode()));
        }

        public static int IndexOf<T>(this IEnumerable<T> self, T value)
        {
            var comparer = EqualityComparer<T>.Default;
            var found = self.Select((el, index) => new { Element = el, Index = index }).FirstOrDefault(i => comparer.Equals(i.Element, value));
            return found?.Index ?? -1;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> self)
        {
            return self == null || !self.Any();
        }

        public static object? Cast(this IEnumerable<object> self, Type type, bool toList = false)
        {
            var enumerableType = typeof(Enumerable);

            var castMethod = enumerableType?.GetMethod("Cast")?.MakeGenericMethod(type);
            var castIterator = castMethod?.Invoke(null, new object[] { self });

            var toCollectionMethod = enumerableType?.GetMethod(toList ? "ToList" : "ToArray")?.MakeGenericMethod(type);
            return toCollectionMethod?.Invoke(null, new[] { castIterator });
        }

        public static IEnumerable<T> Reorder<T>(this IEnumerable<T> self, IEnumerable<T> correctOrder)
        {
            var result = new List<T>();
            var selfList = new List<T>(self);
            var correctOrderList = new List<T>(correctOrder);

            while (selfList.Count > 0)
            {
                if (!correctOrder.Contains(selfList.First()))
                {
                    result.Add(selfList.First());
                }
                else
                {
                    result.Add(correctOrderList.First());
                    correctOrderList.RemoveAt(0);
                }

                selfList.RemoveAt(0);
            }

            return result.ToArray();
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> self, int batchSize)
        {
            var nextBatch = new List<T>(batchSize);
            foreach (T item in self)
            {
                nextBatch.Add(item);
                if (nextBatch.Count == batchSize)
                {
                    yield return nextBatch;
                    nextBatch = new List<T>(batchSize);
                }
            }

            if (nextBatch.Count > 0)
                yield return nextBatch;
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, bool randomize = false)
        {
            return source.MinBy(selector, randomize, Comparer<TKey>.Default);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, bool randomize, IComparer<TKey> comparer)
        {
            if (source == null)
                throw new ArgumentException("source");
            if (selector == null)
                throw new ArgumentException("selector");
            if (comparer == null)
                throw new ArgumentException("comparer");

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                    throw new InvalidOperationException("Sequence was empty");

                var min = sourceIterator.Current;
                var minKey = selector(min);

                if (randomize)
                {
                    var minList = new List<TSource> { min };

                    while (sourceIterator.MoveNext())
                    {
                        var candidate = sourceIterator.Current;
                        var candidateProjected = selector(candidate);
                        var compare = comparer.Compare(candidateProjected, minKey);
                        if (compare == 0)
                        {
                            minList.Add(candidate);
                        }
                        else if (compare < 0)
                        {
                            minList = new List<TSource> { candidate };
                            minKey = candidateProjected;
                        }
                    }

                    return minList.RandomElement();
                }
                else
                {
                    while (sourceIterator.MoveNext())
                    {
                        var candidate = sourceIterator.Current;
                        var candidateProjected = selector(candidate);
                        if (comparer.Compare(candidateProjected, minKey) < 0)
                        {
                            min = candidate;
                            minKey = candidateProjected;
                        }
                    }

                    return min;
                }
            }
        }

        // https://stackoverflow.com/a/1300116/4024004
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                    yield return element;
            }
        }

        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            var secondKeys = new HashSet<TKey>(second.Select(keySelector));
            foreach (var source in first)
            {
                if (secondKeys.Remove(keySelector(source)))
                    yield return source;
            }
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            var secondKeys = new HashSet<TKey>(second.Select(keySelector));
            foreach (var source in first)
            {
                if (secondKeys.Add(keySelector(source)))
                    yield return source;
            }
        }

        public static IEnumerable<TSource> FindDuplicatesBy<TSource, TKey>(this IEnumerable<TSource> self, Func<TSource, TKey> keySelector)
        {
            return self.GroupBy(keySelector).Where(g => g.Count() > 1).SelectMany(g => g);
        }

        #region [RankBy]

        public static IEnumerable<TResult> DenseRankBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, RankingResult, TResult> resultSelector)
        {
            return source.DenseRankBy(keySelector, null, false, resultSelector);
        }

        public static IEnumerable<TResult> DenseRankByDescending<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, RankingResult, TResult> resultSelector)
        {
            return source.DenseRankBy(keySelector, null, true, resultSelector);
        }

        private static IEnumerable<TResult> DenseRankBy<TSource, TKey, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            IComparer<TKey>? comparer,
            bool descending,
            Func<TSource, RankingResult, TResult> resultSelector)
        {
            comparer = comparer ?? Comparer<TKey>.Default;

            var grouped = source.GroupBy(keySelector);
            var ordered =
                descending
                    ? grouped.OrderByDescending(g => g.Key, comparer)
                    : grouped.OrderBy(g => g.Key, comparer);

            var dict = new Dictionary<TSource, RankingResult>();

            var totalRank = 1;
            foreach (var group in ordered)
            {
                var rank = totalRank;
                foreach (var item in group)
                {
                    dict.Add(item, new RankingResult { Rank = rank });
                    totalRank++;
                }
            }

            var count = dict.Count;
            foreach (var item in dict)
            {
                var countRankedBetter = dict.Count(p => p.Value.Rank < item.Value.Rank);
                item.Value.Percentile = count > 0 ? Math.Round((1.0M - ((decimal)countRankedBetter / count)) * 100, 0) : 100M;
                yield return resultSelector(item.Key, item.Value);
            }
        }

        public class RankingResult
        {
            public int Rank { get; set; }
            public decimal Percentile { get; set; }
        }

        #endregion

        #region Random

        private static Random _rand;

        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            if (_rand == null)
                _rand = new Random();

            var current = default(T);
            var count = 0;

            foreach (var element in source)
            {
                count++;

                if (_rand.Next(count) == 0)
                    current = element;
            }

            if (count == 0)
                throw new InvalidOperationException("Sequence was empty");

            return current;
        }

        public static T RandomElement<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Where(predicate).RandomElement();
        }

        // https://stackoverflow.com/a/1287572/4024004
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> self)
        {
            var elements = self.ToArray();
            var random = new Random();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                var swapIndex = random.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        #endregion
    }
}
