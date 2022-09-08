using FakeRpc.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public interface IWebSocketCallInvoker
    {
        Action<FakeRpcRequest> OnSend { get; set; }
        Action<FakeRpcResponse> OnReceive { get; set; }
        Task Invoke(FakeRpcRequest request, WebSocket webSocket, IMessageSerializer serializationHandler);
    }
}
