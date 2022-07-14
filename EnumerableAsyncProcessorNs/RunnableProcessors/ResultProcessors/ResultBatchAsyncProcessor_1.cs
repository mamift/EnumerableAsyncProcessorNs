using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultBatchAsyncProcessor<TOutput> : ResultAbstractAsyncProcessor<TOutput>
    {
        private readonly int _batchSize;

        internal ResultBatchAsyncProcessor(int batchSize, int count, Func<Task<TOutput>> taskSelector,
            CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _batchSize = batchSize;
        }

        internal override async Task Process()
        {
            TaskCompletionSource<TOutput>[][] batchedTaskCompletionSources = EnumerableTaskCompletionSources.Chunk(_batchSize).ToArray();

            foreach (TaskCompletionSource<TOutput>[] currentTaskCompletionSourceBatch in batchedTaskCompletionSources)
            {
                await ProcessBatch(currentTaskCompletionSourceBatch);
            }
        }

        private Task ProcessBatch(TaskCompletionSource<TOutput>[] currentTaskCompletionSourceBatch)
        {
            foreach (TaskCompletionSource<TOutput> taskCompletionSource in currentTaskCompletionSourceBatch)
            {
                _ = ProcessItem(taskCompletionSource);
            }

            return Task.WhenAll(currentTaskCompletionSourceBatch.Select(x => x.Task));
        }
    }
}