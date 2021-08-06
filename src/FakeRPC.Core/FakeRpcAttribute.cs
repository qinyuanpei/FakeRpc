using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcAttribute : Attribute
    {
        public string ServiceName { get; set; }
        public string ServiceGroup { get; set; }
    }
}
