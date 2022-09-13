using FakeRpc.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.IO;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public interface IServerWebSocketCallInvoker
    {
        Task InvokeAsync(Stream stream);

        Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token);
    }
}
