using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcServiceDescriptor
    {
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
    }
}
