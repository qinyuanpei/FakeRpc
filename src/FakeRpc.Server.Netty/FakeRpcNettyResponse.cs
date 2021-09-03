using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Server.Netty
{
    public class FakeRpcNettyResponse
    {
        public string RequestId { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
