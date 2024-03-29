﻿using CSRedis;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.ServiceRegistry.Redis
{
    public class RedisServiceRegistry : BaseServiceRegistry
    {
        private readonly CSRedisClient _redisClient;
        private readonly ILogger<RedisServiceRegistry> _logger;
        private readonly RedisServiceRegistryOptions _options;

        public RedisServiceRegistry(RedisServiceRegistryOptions options,ILogger<RedisServiceRegistry> logger)
        {
            _options = options;
            _redisClient = new CSRedisClient(options.RedisUrl);
            RedisHelper.Initialization(_redisClient);
            _logger = logger;
        }

        public override void Register(ServiceRegistration serviceRegistration)
        {
            var serviceName = serviceRegistration.ServiceName;
            var serviceGroup = serviceRegistration.ServiceGroup;
            var registryKey = $"{Constants.FAKE_RPC_ROUTE_PREFIX}:{serviceGroup.Replace(".",":")}:{serviceName}";
            _logger.LogInformation($"Register {serviceRegistration.ServiceHost}:{serviceRegistration.ServicePort} to {registryKey} ...");
            var serviceNodes = _redisClient.SMembers<ServiceRegistration>(registryKey)?.ToList();
            if (!serviceNodes.Any(x => x.ServiceUri == serviceRegistration.ServiceUri))
            {
                _redisClient.SAdd(registryKey, serviceRegistration);
                Publish(_options.RegisterEventTopic, new { Key = registryKey, Value = serviceRegistration });
            }

        }

        public override void Unregister(ServiceRegistration serviceRegistration)
        {
            var serviceName = serviceRegistration.ServiceName;
            var serviceGroup = serviceRegistration.ServiceGroup;
            var registryKey = $"{Constants.FAKE_RPC_ROUTE_PREFIX}:{serviceGroup.Replace(".", ":")}:{serviceName}";
            _logger.LogInformation($"Unregister {serviceRegistration.ServiceHost}:{serviceRegistration.ServicePort} from {registryKey} ...");
            var serviceNodes = _redisClient.SMembers<ServiceRegistration>(registryKey)?.ToList();
            var serviceNode = serviceNodes.FirstOrDefault(x => x.ServiceUri == serviceRegistration.ServiceUri);
            if (serviceNode != null)
            {
                _redisClient.SRem(registryKey, serviceRegistration);
                Publish(_options.UnregisterEventTopic, new { Key = registryKey, Value = serviceNode });
            }
        }

        private void Publish(string topic, string message)
        {
            _redisClient.Publish(topic, message);
        }

        private void Publish<TMessage>(string topic, TMessage message)
        {
            _redisClient.Publish(topic, JsonConvert.SerializeObject(message));
        }
    }
}
