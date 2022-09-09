using FakeRpc.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public interface IWebSocketCallInvoker
    {
        EventHandler<FakeRpcRequest> OnMessageSent { get; set; }

        EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }

        Action OnConnecting { get; set; }

        Action OnOpened { get; set; }

        Action OnClosed { get; set; }

        Task InvokeAsync(FakeRpcRequest request);

        Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token);
    }
}
