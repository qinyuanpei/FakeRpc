using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using CSRedis;
using Microsoft.Extensions.Logging;
using static CSRedis.CSRedisClient;
using FakeRpc.Core.Discovery;
using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.LoadBalance;

namespace FakeRpc.ServiceRegistry.Redis
{
    public class RedisServiceDiscovery : BaseServiceDiscovey
    {
        private readonly CSRedisClient _redisClient;
        private readonly RedisServiceDiscoveryOptions _options;
        private readonly ILogger<RedisServiceDiscovery> _logger;

        public RedisServiceDiscovery(RedisServiceDiscoveryOptions options, ILogger<RedisServiceDiscovery> logger, ILoadBalanceStrategy loadBalanceStrategy)
            : base(loadBalanceStrategy)
        {
            _options = options;
            _redisClient = new CSRedisClient(options.RedisUrl);
            RedisHelper.Initialization(_redisClient);
            _redisClient.Subscribe((_options.RegisterEventTopic, OnServiceRegister));
            _redisClient.Subscribe((_options.RegisterEventTopic, OnServiceUnregister));
            _logger = logger;
        }

        public override Uri GetService(string serviceName, string serviceGroup)
        {
            var registryKey = $"{Constants.FAKE_RPC_ROUTE_PREFIX}:{serviceGroup.Replace(".", ":")}:{serviceName}";
            var serviceNodes = _redisClient.SMembers<ServiceRegistration>(registryKey);
            if (serviceNodes == null)
                throw new ArgumentException($"Service {serviceGroup}.{serviceName} can't be resolved.");

            _logger.LogInformation($"Discovery {serviceNodes.Count()} instances for {serviceName} ...");
            var serviceUrls = serviceNodes.Select(x => x.ServiceUri);

            return _loadBalanceStrategy.Select(serviceUrls);
        }

        private void OnServiceRegister(SubscribeMessageEventArgs args)
        {

        }

        private void OnServiceUnregister(SubscribeMessageEventArgs args)
        {

        }
    }
}
