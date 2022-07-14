using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract
{
    public abstract class ResultAbstractAsyncProcessor<TInput, TOutput> : ResultAbstractAsyncProcessorBase<TOutput>
    {
        protected readonly IEnumerable<Tuple<TInput, TaskCompletionSource<TOutput>>> ItemisedTaskCompletionSourceContainers;

        private readonly Func<TInput, Task<TOutput>> _taskSelector;

        protected ResultAbstractAsyncProcessor(IReadOnlyCollection<TInput> items, Func<TInput, Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(items.Count, cancellationTokenSource)
        {
            ItemisedTaskCompletionSourceContainers = items.Select((item, index) =>
                new Tuple<TInput, TaskCompletionSource<TOutput>>(item, EnumerableTaskCompletionSources[index]));
            _taskSelector = taskSelector;
        }

        protected async Task ProcessItem(Tuple<TInput, TaskCompletionSource<TOutput>> itemTaskCompletionSourceTuple)
        {
            (TInput item, TaskCompletionSource<TOutput> taskCompletionSource) = itemTaskCompletionSourceTuple;
            try
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled(CancellationToken);
                    return;
                }

                TOutput result = await _taskSelector(item);
                taskCompletionSource.TrySetResult(result);
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        }
    }
}