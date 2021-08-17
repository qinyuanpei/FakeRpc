using Consul;
using CSRedis;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.LoadBalance;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using FakeRpc.ServiceDiscovery.Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.ServiceDiscovery.Consul
{
    public class ConsulServiceDiscovery : BaseServiceDiscovey
    {
        private readonly IConsulClient _consulClient;

        public ConsulServiceDiscovery(ConsulServiceDiscoveryOptions options, ILoadBalanceStrategy loadBalanceStrategy) 
            : base(loadBalanceStrategy)
        {
            _consulClient = new ConsulClient(new ConsulClientConfiguration() { Address = new Uri(options.BaseUrl) });
        }

        public override ServiceRegistration GetService(string serviceName, string serviceGroup)
        {
            var services = AsyncHelper.RunSync<QueryResult<ServiceEntry[]>>(() => _consulClient.Health.Service(serviceName));
            var serviceUrls = services.Response
                .ToList()
                .Where(x => x.Service.GetServiceGroup() == serviceGroup)
                .Select(x => new ServiceRegistration()
                {
                    ServiceName = x.Service.Service,
                    ServiceUri = new Uri($"{x.Service.GetServiceSchema()}://{x.Service.Address}:{x.Service.Port}"),
                    ServiceGroup = x.Service.Meta[Constants.FAKE_RPC_SERVICE_GROUP],
                    ServiceInterface = x.Service.Meta[Constants.FAKE_RPC_SERVICE_INTERFACE],
                    ServiceProtocols = x.Service.Meta[Constants.FAKE_RPC_SERVICE_PROTOCOLS],
                    ServiceWeight = 1
                });

            return _loadBalanceStrategy.Select(serviceUrls);
        }
    }
}
