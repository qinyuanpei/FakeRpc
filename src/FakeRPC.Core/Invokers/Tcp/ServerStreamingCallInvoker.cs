using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public class ServerStreamingCallInvoker : IServerStreamingCallInvoker
    {
        public Task InvokeAsync(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
