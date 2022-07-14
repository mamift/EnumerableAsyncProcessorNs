using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultRateLimitedParallelAsyncProcessor<TInput, TOutput> : ResultAbstractAsyncProcessor<TInput, TOutput>
    {
        private readonly int _levelsOfParallelism;

        internal ResultRateLimitedParallelAsyncProcessor(IReadOnlyCollection<TInput> items, Func<TInput, Task<TOutput>> taskSelector, int levelsOfParallelism, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
        }

        internal override Task Process()
        {
            return ItemisedTaskCompletionSourceContainers.ParallelForEachAsync(
                asyncItemAction: async delegate(Tuple<TInput, TaskCompletionSource<TOutput>> tuple, long l) { await ProcessItem(tuple); }, 
                maxDegreeOfParallelism: _levelsOfParallelism, 
                cancellationToken: CancellationToken);

            //return Parallel.ForEachAsync(ItemisedTaskCompletionSourceContainers,
            //    new ParallelOptions { MaxDegreeOfParallelism = _levelsOfParallelism, CancellationToken = CancellationToken },
            //    async (itemTaskCompletionSourceTuple, _) =>
            //    {
            //        await ProcessItem(itemTaskCompletionSourceTuple);
            //    });
        }
    }
}