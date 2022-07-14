using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class BatchAsyncProcessor : AbstractAsyncProcessor
    {
        private readonly int _batchSize;

        internal BatchAsyncProcessor(int batchSize, int count, Func<Task> taskSelector, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
            _batchSize = batchSize;
        }

        internal override async Task Process()
        {
            TaskCompletionSource[][] batchedTaskCompletionSources = EnumerableTaskCompletionSources.Chunk(_batchSize).ToArray();

            foreach (TaskCompletionSource[] currentTaskCompletionSourceBatch in batchedTaskCompletionSources)
            {
                await ProcessBatch(currentTaskCompletionSourceBatch);
            }
        }

        private Task ProcessBatch(TaskCompletionSource[] currentTaskCompletionSourceBatch)
        {
            foreach (TaskCompletionSource taskCompletionSource in currentTaskCompletionSourceBatch)
            {
                _ = ProcessItem(taskCompletionSource);
            }

            return Task.WhenAll(currentTaskCompletionSourceBatch.Select(x => x.Task));
        }
    }
}