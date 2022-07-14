using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class BatchAsyncProcessor<TInput> : AbstractAsyncProcessor<TInput>
    {
        private readonly int _batchSize;

        internal BatchAsyncProcessor(int batchSize, ImmutableList<TInput> items, Func<TInput, Task> taskSelector,
            CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _batchSize = batchSize;
        }

        internal override async Task Process()
        {
            Tuple<TInput, TaskCompletionSource>[][] batchedItems = ItemisedTaskCompletionSourceContainers.Chunk(_batchSize).ToArray();

            foreach (Tuple<TInput, TaskCompletionSource>[] currentBatch in batchedItems)
            {
                await ProcessBatch(currentBatch);
            }
        }

        private Task ProcessBatch(Tuple<TInput, TaskCompletionSource>[] currentBatch)
        {
            foreach (Tuple<TInput, TaskCompletionSource> currentItem in currentBatch)
            {
                _ = ProcessItem(currentItem);
            }

            return Task.WhenAll(currentBatch.Select(x =>
            {
                (_, TaskCompletionSource taskCompletionSource) = x;
                return taskCompletionSource.Task;
            }));
        }
    }
}