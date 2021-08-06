using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public class NacosServiceDiscoveryOptions
    {
        public List<string> ServerAddress { get; set; }
    }
}
