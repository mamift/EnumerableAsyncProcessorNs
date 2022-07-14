using System;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class TimedRateLimitedParallelAsyncProcessor : AbstractAsyncProcessor
    {
        private readonly int _levelsOfParallelism;
        private readonly TimeSpan _timeSpan;

        internal TimedRateLimitedParallelAsyncProcessor(int count, Func<Task> taskSelector, int levelsOfParallelism,
            TimeSpan timeSpan, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
            _timeSpan = timeSpan;
        }

        internal override Task Process()
        {
            return EnumerableTaskCompletionSources.ParallelForEachAsync(
                asyncItemAction: async delegate(TaskCompletionSource source, long l) {
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
            //            Task.Delay(_timeSpan, CancellationToken)
            //        );
            //    });
        }
    }
}