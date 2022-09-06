using FakeRpc.Core;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.WebSockets
{
    public interface ISocketRpcBinder
    {
        Action<FakeRpcRequest> OnSend { get; set; }
        Action<FakeRpcResponse<dynamic>> OnReceive { get; set; }
        Task Invoke(FakeRpcRequest request, WebSocket webSocket);
    }
}
