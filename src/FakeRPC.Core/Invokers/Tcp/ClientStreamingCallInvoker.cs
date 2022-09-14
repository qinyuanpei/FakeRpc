using FakeRpc.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public class ClientStreamingCallInvoker : IClientStreamingCallInvoker
    {
        private readonly TcpClient _tcpClient;

        private readonly IServiceProvider _serviceProvider;

        private readonly IMessageSerializer _messageSerializer;

        public ClientStreamingCallInvoker(IServiceProvider serviceProvider, TcpClient tcpClient, IMessageSerializer messageSerializer)
        {
            _tcpClient = tcpClient;
            _serviceProvider = serviceProvider;
            _messageSerializer = messageSerializer;
        }

        public Task InvokeAsync(FakeRpcRequest fakeRpcRequest)
        {
            return Task.CompletedTask;
        }
    }
}
