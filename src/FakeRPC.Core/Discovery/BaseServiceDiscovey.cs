using FakeRpc.Core.LoadBalance;
using FakeRpc.Core.Mics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Discovery
{
    public class BaseServiceDiscovey : IServiceDiscovery
    {
        protected readonly ILoadBalanceStrategy _loadBalanceStrategy;
        public BaseServiceDiscovey(ILoadBalanceStrategy loadBalanceStrategy)
        {
            _loadBalanceStrategy = loadBalanceStrategy;
        }

        public Uri GetService<TService>(string serviceGroup = null)
        {
            if (string.IsNullOrEmpty(serviceGroup))
                serviceGroup = typeof(TService).GetServiceGroup();

            var serviceName = typeof(TService).GetServiceName();
            return GetService(serviceName, serviceGroup);
        }

        public virtual Uri GetService(string serviceName, string serviceGroup)
        {
            throw new NotImplementedException();
        }
    }
}
