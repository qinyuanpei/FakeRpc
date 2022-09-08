using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Binder.Http
{
    public interface IHtttpCallInvoker
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(Uri uri, TRequest request);
        Task<TResponse> CallAsync<TResponse>(Uri uri);
    }
}
