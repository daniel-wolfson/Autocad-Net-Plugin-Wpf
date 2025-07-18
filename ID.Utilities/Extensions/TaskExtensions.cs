using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ID.Infrastructure.Helpers
{
    /// <summary> Async </summary>
    public static class TaskExtensions
    {
        public static Task LoopAsync<T>(this IEnumerable<T> list, Func<T, Task> function)
        {
            return Task.WhenAll(list.Select(function));
        }

        public async static Task<IEnumerable<TOut>> LoopAsyncResult<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, Task<TOut>> function)
        {
            TOut[] loopResult = await Task.WhenAll(list.Select(function));
            return loopResult.ToList().AsEnumerable();
        }
    }
}
