using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Server.Netty
{
    public class FakeRpcNettyRequest
    {
        public string RequestId { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public object[] Parameters { get; set; }
    }
}
