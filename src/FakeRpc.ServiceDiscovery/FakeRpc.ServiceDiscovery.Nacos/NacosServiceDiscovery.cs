using FakeRpc.Core.Discovery;
using System;
using System.Collections.Generic;
using Nacos.V2;
using FakeRpc.Core.Mics;
using Nacos.V2.Naming.Dtos;
using System.Linq;
using FakeRpc.Core.LoadBalance;
using FakeRpc.Core;

namespace FakeRpc.ServiceDiscovery.Nacos
{
    public class NacosServiceDiscovery : BaseServiceDiscovey
    {
        private readonly INacosNamingService _nacosNamingService;

        public NacosServiceDiscovery(INacosNamingService nacosNamingService, ILoadBalanceStrategy loadBalanceStrategy)
            : base(loadBalanceStrategy)
        {
            _nacosNamingService = nacosNamingService;
        }

        public override ServiceRegistration GetService(string serviceName, string serviceGroup)
        {
            var instances = AsyncHelper.RunSync<List<Instance>>(() => _nacosNamingService.GetAllInstances(serviceName, serviceGroup));
            var serviceRegistrations = instances
                .Where(x => x.Healthy)
                .Select(x => new ServiceRegistration
                {
                    ServiceName = x.ServiceName,
                    ServiceUri = new Uri($"{x.GetServiceSchema()}://{x.Ip}:{x.Port}"),
                    ServiceInterface = x.Metadata[Constants.FAKE_RPC_SERVICE_INTERFACE],
                    ServiceProtocols =  x.Metadata[Constants.FAKE_RPC_SERVICE_PROTOCOLS],
                    ServiceGroup = x.Metadata[Constants.FAKE_RPC_SERVICE_GROUP],
                    ServiceWeight = (int)x.Weight * 100,
                });

            return _loadBalanceStrategy.Select(serviceRegistrations);
        }
    }
}
