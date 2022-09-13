using System;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Core.Mics
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _factory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(Func<Task> func)
        {
            _factory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _factory.StartNew(func).Unwrap().GetAwaiter().GetResult();
        }
    }

}