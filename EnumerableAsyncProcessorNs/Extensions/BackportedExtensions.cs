using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;

namespace EnumerableAsyncProcessor.Ns.Extensions
{
    public static class BackportedExtensions
    {
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
        {
            IEnumerable<IEnumerable<T>> batched = source.Batch(size);

            foreach (var batch in batched) {
                yield return batch.ToArray();
            }
        }
    }
}