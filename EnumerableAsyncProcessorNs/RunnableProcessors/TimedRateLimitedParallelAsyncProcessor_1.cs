using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class TimedRateLimitedParallelAsyncProcessor<TInput> : AbstractAsyncProcessor<TInput>
    {
        private readonly int _levelsOfParallelism;
        private readonly TimeSpan _timeSpan;

        internal TimedRateLimitedParallelAsyncProcessor(ImmutableList<TInput> items, Func<TInput, Task> taskSelector, int levelsOfParallelism, TimeSpan timeSpan, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
            _timeSpan = timeSpan;
        }

        internal override Task Process()
        {
            return ItemisedTaskCompletionSourceContainers.ParallelForEachAsync(
                asyncItemAction: async delegate(Tuple<TInput, TaskCompletionSource> tuple, long l) {
                    var processTask = ProcessItem(tuple);
                    var delayTask = Task.Delay(_timeSpan, CancellationToken);
                    await Task.WhenAll(processTask, delayTask);
                },
                maxDegreeOfParallelism: _levelsOfParallelism,
                cancellationToken: CancellationToken
            );

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