using FakeRpc.Core.Discovery;
using System;
using System.Collections.Generic;
using Nacos.V2;
using FakeRpc.Core.Mics;
using Nacos.V2.Naming.Dtos;
using System.Linq;
using FakeRpc.Core.LoadBalance;

namespace FakeRpc.ServiceDiscovery.Nacos
{
    public class NacosServiceDiscovery: BaseServiceDiscovey
    {
        private readonly INacosNamingService _nacosNamingService;

        public NacosServiceDiscovery(INacosNamingService nacosNamingService, ILoadBalanceStrategy loadBalanceStrategy)
            : base(loadBalanceStrategy)
        {
            _nacosNamingService = nacosNamingService;
        }

        public override Uri GetService(string serviceName, string serviceGroup)
        {
            var instances = AsyncHelper.RunSync<List<Instance>>(() => _nacosNamingService.GetAllInstances(serviceName, serviceGroup));
            var serviceUrls = instances
                .Where(x => x.Healthy)
                .Select(x => new Uri($"{x.GetServiceSchema()}://{x.Ip}:{x.Port}"));

            return _loadBalanceStrategy.Select(serviceUrls);
        }
    }
}
