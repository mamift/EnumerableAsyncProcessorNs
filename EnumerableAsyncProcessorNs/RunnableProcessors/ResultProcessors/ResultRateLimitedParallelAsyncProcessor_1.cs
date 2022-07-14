using System;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultRateLimitedParallelAsyncProcessor<TOutput> : ResultAbstractAsyncProcessor<TOutput>
    {
        private readonly int _levelsOfParallelism;

        internal ResultRateLimitedParallelAsyncProcessor(int count, Func<Task<TOutput>> taskSelector, int levelsOfParallelism, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
        }

        internal override Task Process()
        {
            return EnumerableTaskCompletionSources.ParallelForEachAsync(
                asyncItemAction: async delegate(TaskCompletionSource<TOutput> source, long l) { await ProcessItem(source); },
                maxDegreeOfParallelism: _levelsOfParallelism, 
                cancellationToken: CancellationToken);

            //return Parallel.ForEachAsync(EnumerableTaskCompletionSources,
            //    new ParallelOptions
            //    { MaxDegreeOfParallelism = _levelsOfParallelism, CancellationToken = CancellationToken },
            //    async (taskCompletionSource, _) =>
            //    {
            //        await ProcessItem(taskCompletionSource);
            //    });
        }
    }
}