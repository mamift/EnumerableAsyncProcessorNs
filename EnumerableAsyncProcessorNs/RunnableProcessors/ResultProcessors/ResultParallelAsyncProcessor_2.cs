using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultParallelAsyncProcessor<TInput, TOutput> : ResultRateLimitedParallelAsyncProcessor<TInput, TOutput>
    {
        internal ResultParallelAsyncProcessor(ImmutableList<TInput> items, Func<TInput, Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(items, taskSelector, -1, cancellationTokenSource)
        {
        }
    }
}