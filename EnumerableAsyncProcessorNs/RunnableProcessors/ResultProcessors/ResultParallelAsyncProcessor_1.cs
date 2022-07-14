﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors
{
    public class ResultParallelAsyncProcessor<TOutput> : ResultRateLimitedParallelAsyncProcessor<TOutput>
    {
        internal ResultParallelAsyncProcessor(int count, Func<Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(count, taskSelector, -1, cancellationTokenSource)
        {
        }
    }
}