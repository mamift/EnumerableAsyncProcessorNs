using EnumerableAsyncProcessor.Ns.Interfaces;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.Abstract;
using EnumerableAsyncProcessor.Ns.RunnableProcessors.ResultProcessors.Abstract;

namespace EnumerableAsyncProcessor.Ns.Extensions
{
    internal static class AsyncProcessorExtensions
    {
        internal static IAsyncProcessor StartProcessing(this AbstractAsyncProcessorBase processor)
        {
            _ = processor.Process();
            return processor;
        }

        internal static IAsyncProcessor<T1> StartProcessing<T1>(this ResultAbstractAsyncProcessorBase<T1> processor)
        {
            _ = processor.Process();
            return processor;
        }
    }
}