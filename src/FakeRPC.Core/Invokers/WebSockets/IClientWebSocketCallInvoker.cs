using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public interface IClientWebSocketCallInvoker
    {
        EventHandler<FakeRpcRequest> OnMessageSent { get; set; }

        EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }

        Action OnConnecting { get; set; }

        Action OnOpened { get; set; }

        Action<WebSocketClosedEventArgs> OnClosed { get; set; }

        Task InvokeAsync(FakeRpcRequest request);

        Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token);
    }

    public class WebSocketClosedEventArgs
    {
        public WebSocketCloseStatus CloseStatus { get; set; }
        public string StatusDescription { get; set; }
    }
}
