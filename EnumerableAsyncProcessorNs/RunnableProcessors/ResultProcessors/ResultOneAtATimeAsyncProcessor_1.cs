using System;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultOneAtATimeAsyncProcessor<TOutput> : ResultAbstractAsyncProcessor<TOutput>
    {
        internal ResultOneAtATimeAsyncProcessor(int count, Func<Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
        }

        internal override async Task Process()
        {
            foreach (TaskCompletionSource<TOutput> taskCompletionSource in EnumerableTaskCompletionSources)
            {
                await ProcessItem(taskCompletionSource);
            }
        }
    }
}