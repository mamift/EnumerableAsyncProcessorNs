using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultBatchAsyncProcessor<TInput, TOutput> : ResultAbstractAsyncProcessor<TInput, TOutput>
    {
        private readonly int _batchSize;

        internal ResultBatchAsyncProcessor(int batchSize, IReadOnlyCollection<TInput> items, Func<TInput, Task<TOutput>> taskSelector,
            CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
            _batchSize = batchSize;
        }

        internal override async Task Process()
        {
            Tuple<TInput, TaskCompletionSource<TOutput>>[][] batchedItems = ItemisedTaskCompletionSourceContainers.Chunk(_batchSize).ToArray();

            foreach (Tuple<TInput, TaskCompletionSource<TOutput>>[] currentBatch in batchedItems)
            {
                await ProcessBatch(currentBatch);
            }
        }

        private Task ProcessBatch(Tuple<TInput, TaskCompletionSource<TOutput>>[] currentBatch)
        {
            foreach (Tuple<TInput, TaskCompletionSource<TOutput>> currentItem in currentBatch)
            {
                _ = ProcessItem(currentItem);
            }

            return Task.WhenAll(currentBatch.Select(x =>
            {
                (_, TaskCompletionSource<TOutput> taskCompletionSource) = x;
                return taskCompletionSource.Task;
            }));
        }
    }
}