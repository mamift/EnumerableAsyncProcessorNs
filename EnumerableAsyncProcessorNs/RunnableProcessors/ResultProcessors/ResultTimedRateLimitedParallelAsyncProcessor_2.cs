using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultTimedRateLimitedParallelAsyncProcessor<TInput, TOutput> : ResultAbstractAsyncProcessor<TInput, TOutput>
    {
        private readonly int _levelsOfParallelism;
        private readonly TimeSpan _timeSpan;

        internal ResultTimedRateLimitedParallelAsyncProcessor(IReadOnlyCollection<TInput> items,
            Func<TInput, Task<TOutput>> taskSelector, int levelsOfParallelism, TimeSpan timeSpan,
            CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
            _timeSpan = timeSpan;
        }

        internal override Task Process()
        {
            return ItemisedTaskCompletionSourceContainers.ParallelForEachAsync(
                asyncItemAction: async delegate(Tuple<TInput, TaskCompletionSource<TOutput>> tuple, long l) {
                    var processTask = ProcessItem(tuple);
                    var delayTask = Task.Delay(_timeSpan, CancellationToken);

                    await Task.WhenAll(processTask, delayTask);
                },
                maxDegreeOfParallelism: _levelsOfParallelism,
                cancellationToken: CancellationToken);

            //return Parallel.ForEachAsync(ItemisedTaskCompletionSourceContainers,
            //    new ParallelOptions { MaxDegreeOfParallelism = _levelsOfParallelism, CancellationToken = CancellationToken },
            //    async (itemTaskCompletionSourceTuple, _) =>
            //    {
            //        await Task.WhenAll(
            //            ProcessItem(itemTaskCompletionSourceTuple),
            //            Task.Delay(_timeSpan, CancellationToken)
            //        );
            //    });
        }
    }
}