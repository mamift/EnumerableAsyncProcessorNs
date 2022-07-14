using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class OneAtATimeAsyncProcessor<TInput> : AbstractAsyncProcessor<TInput>
    {
        internal OneAtATimeAsyncProcessor(ImmutableList<TInput> items, Func<TInput, Task> taskSelector, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, cancellationTokenSource)
        {
        }

        internal override async Task Process()
        {
            foreach (Tuple<TInput, TaskCompletionSource> itemTaskCompletionSourceTuple in ItemisedTaskCompletionSourceContainers)
            {
                await ProcessItem(itemTaskCompletionSourceTuple);
            }
        }
    }
}