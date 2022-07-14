using System;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultTimedRateLimitedParallelAsyncProcessor<TOutput> : ResultAbstractAsyncProcessor<TOutput>
    {
        private readonly int _levelsOfParallelism;
        private readonly TimeSpan _timeSpan;

        internal ResultTimedRateLimitedParallelAsyncProcessor(int count, Func<Task<TOutput>> taskSelector, int levelsOfParallelism, TimeSpan timeSpan, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
            _timeSpan = timeSpan;
        }

        internal override Task Process()
        {
            return EnumerableTaskCompletionSources.ParallelForEachAsync(
                asyncItemAction: async delegate(TaskCompletionSource<TOutput> source, long l) {
                    var processTask = ProcessItem(source);
                    var delayTask = Task.Delay(_timeSpan, CancellationToken);
                    await Task.WhenAll(processTask, delayTask);
                },
                maxDegreeOfParallelism: _levelsOfParallelism,
                cancellationToken: CancellationToken);

            //return Parallel.ForEachAsync(EnumerableTaskCompletionSources,
            //    new ParallelOptions
            //    { MaxDegreeOfParallelism = _levelsOfParallelism, CancellationToken = CancellationToken },
            //    async (taskCompletionSource, _) =>
            //    {
            //        await Task.WhenAll(
            //            ProcessItem(taskCompletionSource),
            //            Task.Delay(_timeSpan, CancellationToken));
            //    });
        }
    }
}