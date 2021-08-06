using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.ServiceRegistry.Consul
{
    public class ConsulServiceDiscoveryOptions
    {
        public string BaseUrl{ get; set; }
        public bool UseHttps { get; set; } = true;
    }
}
