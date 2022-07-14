using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultOneAtATimeAsyncProcessor<TInput, TOutput> : ResultAbstractAsyncProcessor<TInput, TOutput>
    {
        internal ResultOneAtATimeAsyncProcessor(IReadOnlyCollection<TInput> items, Func<TInput, Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
        }

        internal override async Task Process()
        {
            foreach (Tuple<TInput, TaskCompletionSource<TOutput>> itemTaskCompletionSourceTuple in ItemisedTaskCompletionSourceContainers)
            {
                await ProcessItem(itemTaskCompletionSourceTuple);
            }
        }
    }
}