using System;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.Interfaces;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors;

namespace EnumerableAsyncProcessor.Ns.Builders
{
    public class ActionAsyncProcessorBuilder<TOutput>
    {
        private readonly int _count;
        private readonly Func<Task<TOutput>> _taskSelector;
        private readonly CancellationTokenSource _cancellationTokenSource;

        internal ActionAsyncProcessorBuilder(int count, Func<Task<TOutput>> taskSelector, CancellationToken cancellationToken)
        {
            _count = count;
            _taskSelector = taskSelector;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public IAsyncProcessor<TOutput> ProcessInBatches(int batchSize)
        {
            return new ResultBatchAsyncProcessor<TOutput>(batchSize, _count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel(int levelOfParallelism)
        {
            return new ResultRateLimitedParallelAsyncProcessor<TOutput>(_count, _taskSelector, levelOfParallelism, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel(int levelOfParallelism, TimeSpan timeSpan)
        {
            return new ResultTimedRateLimitedParallelAsyncProcessor<TOutput>(_count, _taskSelector, levelOfParallelism, timeSpan, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessInParallel()
        {
            return new ResultParallelAsyncProcessor<TOutput>(_count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor<TOutput> ProcessOneAtATime()
        {
            return new ResultOneAtATimeAsyncProcessor<TOutput>(_count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }
    }
}