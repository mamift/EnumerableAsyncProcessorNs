using System;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class RateLimitedParallelAsyncProcessor : AbstractAsyncProcessor
    {
        private readonly int _levelsOfParallelism;

        internal RateLimitedParallelAsyncProcessor(int count, Func<Task> taskSelector, int levelsOfParallelism,
            CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _levelsOfParallelism = levelsOfParallelism;
        }

        internal override Task Process()
        {
            return EnumerableTaskCompletionSources.ParallelForEachAsync(
                asyncItemAction: async delegate(TaskCompletionSource source, long index) { await ProcessItem(source); },
                maxDegreeOfParallelism: _levelsOfParallelism,
                cancellationToken: CancellationToken
            );

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