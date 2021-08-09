using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public class NacosServiceRegistryOptions
    {
        public string Namespace { get; set; } = string.Empty;
        public List<string> ServerAddress { get; set; }
    }
}
