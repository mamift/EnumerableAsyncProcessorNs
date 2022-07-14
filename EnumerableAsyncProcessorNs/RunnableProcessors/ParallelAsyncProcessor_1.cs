using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors
{
    public class ParallelAsyncProcessor<TInput> : RateLimitedParallelAsyncProcessor<TInput>
    {
        internal ParallelAsyncProcessor(ImmutableList<TInput> items, Func<TInput, Task> taskSelector, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, -1, cancellationTokenSource)
        {
        }
    }
}