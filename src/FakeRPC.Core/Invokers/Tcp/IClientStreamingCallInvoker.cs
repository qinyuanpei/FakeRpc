using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public interface IClientStreamingCallInvoker
    {
        Task InvokeAsync(FakeRpcRequest fakeRpcRequest);
    }
}
