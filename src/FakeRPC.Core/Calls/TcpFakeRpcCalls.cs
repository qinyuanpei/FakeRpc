using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Calls
{
    class TcpFakeRpcCalls : IFakeRpcCalls
    {
        private readonly HttpClient _httpClient;
        private readonly TcpClient _tcpCLient;
        public TcpFakeRpcCalls(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<TResponse> CallAsync<TRequest, TResponse>(Uri uri, TRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> CallAsync<TResponse>(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
