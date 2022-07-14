using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using EnumerableAsyncProcessor.Ns.Builders;

namespace EnumerableAsyncProcessor.Ns.Extensions
{
    public static class EnumerableExtensions
    {
        public static ItemAsyncProcessorBuilder<T> ToAsyncProcessorBuilder<T>(this IEnumerable<T> items)
        {
            return new ItemAsyncProcessorBuilder<T>(items);
        }

        internal static IAsyncEnumerable<T> ToIAsyncEnumerable<T>(this IEnumerable<Task<T>> tasks)
        {
            return new AsyncEnumerable<T>(async delegate(AsyncEnumerator<T>.Yield yielder) {
                List<Task<T>> managedTasksList = tasks.ToList();

                while (managedTasksList.Any()) {
                    Task<T> finishedTask = await Task.WhenAny(managedTasksList);
                    managedTasksList.Remove(finishedTask);
                    var result = await finishedTask;

                    await yielder.ReturnAsync(result);
                }
            });
        }
    }
}