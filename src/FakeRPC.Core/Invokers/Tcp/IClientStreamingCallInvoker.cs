using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public interface IClientStreamingCallInvoker
    {
        EventHandler<FakeRpcRequest> OnMessageSent { get; set; }

        EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }

        Task InvokeAsync(FakeRpcRequest fakeRpcRequest);
    }
}
