using System;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.Extensions;
using EnumerableAsyncProcessor.Ns.Interfaces;
using EnumerableAsyncProcessor.Ns.RunnableProcessors;

namespace EnumerableAsyncProcessor.Ns.Builders
{
    public class ActionAsyncProcessorBuilder
    {
        private readonly int _count;
        private readonly Func<Task> _taskSelector;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ActionAsyncProcessorBuilder(int count, Func<Task> taskSelector, CancellationToken cancellationToken)
        {
            _count = count;
            _taskSelector = taskSelector;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public IAsyncProcessor ProcessInBatches(int batchSize)
        {
            return new BatchAsyncProcessor(batchSize, _count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor ProcessInParallel(int levelOfParallelism)
        {
            return new RateLimitedParallelAsyncProcessor(_count, _taskSelector, levelOfParallelism, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor ProcessInParallel(int levelOfParallelism, TimeSpan timeSpan)
        {
            return new TimedRateLimitedParallelAsyncProcessor(_count, _taskSelector, levelOfParallelism, timeSpan, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor ProcessInParallel()
        {
            return new ParallelAsyncProcessor(_count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }

        public IAsyncProcessor ProcessOneAtATime()
        {
            return new OneAtATimeAsyncProcessor(_count, _taskSelector, _cancellationTokenSource).StartProcessing();
        }
    }
}