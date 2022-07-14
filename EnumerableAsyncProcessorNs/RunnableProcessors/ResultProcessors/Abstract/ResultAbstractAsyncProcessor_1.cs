﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract
{
    public abstract class ResultAbstractAsyncProcessor<TOutput> : ResultAbstractAsyncProcessorBase<TOutput>
    {
        private readonly Func<Task<TOutput>> _taskSelector;

        protected ResultAbstractAsyncProcessor(int count, Func<Task<TOutput>> taskSelector, CancellationTokenSource cancellationTokenSource) : base(count, cancellationTokenSource)
        {
            _taskSelector = taskSelector;
        }

        protected async Task ProcessItem(TaskCompletionSource<TOutput> taskCompletionSource)
        {
            try
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled(CancellationToken);
                    return;
                }

                TOutput result = await _taskSelector();
                taskCompletionSource.TrySetResult(result);
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        }
    }
}