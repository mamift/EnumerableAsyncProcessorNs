using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class RateLimitedParallelAsyncProcessor<TInput> : AbstractAsyncProcessor<TInput>
    {
        private readonly int _levelsOfParallelism;

        internal RateLimitedParallelAsyncProcessor(ImmutableList<TInput> items, Func<TInput, Task> taskSelector,
            int levelsOfParallelism, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
        }

        internal override Task Process()
        {
            return ItemisedTaskCompletionSourceContainers.ParallelForEachAsync(
                asyncItemAction: async delegate(Tuple<TInput, TaskCompletionSource> tuple, long l) { await ProcessItem(tuple); },
                maxDegreeOfParallelism: _levelsOfParallelism,
                cancellationToken: CancellationToken
            );

            //return Parallel.ForEachAsync(ItemisedTaskCompletionSourceContainers,
            //    new ParallelOptions { MaxDegreeOfParallelism = _levelsOfParallelism, CancellationToken = CancellationToken },
            //    async (itemTaskCompletionSourceTuple, _) =>
            //    {
            //        await ProcessItem(itemTaskCompletionSourceTuple);
            //    });
        }
    }
}