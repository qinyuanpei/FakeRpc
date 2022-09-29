using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Binder.Http
{
    public interface IHtttpCallInvoker
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(Uri uri, TRequest request) where TRequest : class where TResponse : class;
        Task<TResponse> CallAsync<TResponse>(Uri uri) where TResponse : class;
    }
}
