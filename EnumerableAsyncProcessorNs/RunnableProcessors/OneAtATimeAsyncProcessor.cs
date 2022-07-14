using System;
using System.Threading;
using System.Threading.Tasks;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class OneAtATimeAsyncProcessor : AbstractAsyncProcessor
    {
        internal OneAtATimeAsyncProcessor(int count, Func<Task> taskSelector, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, cancellationTokenSource)
        {
        }

        internal override async Task Process()
        {
            foreach (TaskCompletionSource taskCompletionSource in EnumerableTaskCompletionSources)
            {
                await ProcessItem(taskCompletionSource);
            }
        }
    }
}