using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.Interfaces;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors;

namespace EnumerableAsyncProcessor.Ns.Builders
{
    public class ItemActionAsyncProcessorBuilder<TInput, TOutput>
    {
        private readonly ImmutableList<TInput> _items;
        private readonly Func<TInput, Task<TOutput>> _taskSelector;
        private readonly CancellationTokenSource _cancellationTokenSource;

        internal ItemActionAsyncProcessorBuilder(IEnumerable<TInput> items, Func<TInput, Task<TOutput>> taskSelector, CancellationToken cancellationToken)
        {
            _items = items.ToImmutableList();
            _taskSelector = taskSelector;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public IAsyncProcessor<TOutput> ProcessInBatches(int batchSize)
        {
            return new ResultBatchAsyncProcessor<TInput, TOutput>(batchSize, _items, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel(int levelOfParallelism)
        {
            return new ResultRateLimitedParallelAsyncProcessor<TInput, TOutput>(_items, _taskSelector, levelOfParallelism, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel(int levelOfParallelism, TimeSpan timeSpan)
        {
            return new ResultTimedRateLimitedParallelAsyncProcessor<TInput, TOutput>(_items, _taskSelector, levelOfParallelism, timeSpan, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel()
        {
            return new ResultParallelAsyncProcessor<TInput, TOutput>(_items, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessOneAtATime()
        {
            return new ResultOneAtATimeAsyncProcessor<TInput, TOutput>(_items, _taskSelector, _cancellationTokenSource).StartProcessing();
        }
    }
}