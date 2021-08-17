using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Discovery
{
    public interface IServiceDiscovery
    {
        ServiceRegistration GetService<TService>(string serviceNGroup = null);
        ServiceRegistration GetService(string serviceName, string serviceGroup);
    }
}
