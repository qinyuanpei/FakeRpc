using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using Microsoft.Extensions.Logging;
using Nacos;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public class NacosServiceRegistry : BaseServiceRegistry
    {
        private readonly INacosNamingService _nacosNamingService;
        private readonly ILogger<NacosServiceRegistry> _logger;

        public NacosServiceRegistry(INacosNamingService nacosNamingService, ILogger<NacosServiceRegistry> logger)
        {
            _nacosNamingService = nacosNamingService;
            _logger = logger;
        }

        public override void Register(ServiceRegistration serviceRegistration)
        {
            var instance = new Instance()
            {
                Ip = serviceRegistration.ServiceHost,
                Port = serviceRegistration.ServicePort,
                ServiceName = serviceRegistration.ServiceName,
                ClusterName = $"{serviceRegistration.ServiceHost}:{serviceRegistration.ServicePort}",
                InstanceId = serviceRegistration.GetServiceId(),
                Ephemeral = false,
                Healthy = true,
                Enabled = true,
                Metadata = new Dictionary<string, string>()
                {
                    { Constants.FAKE_RPC_SERVICE_GROUP, serviceRegistration.ServiceGroup },
                    { Constants.FAKE_RPC_SERVICE_SCHEMA,serviceRegistration.ServiceSchema },
                    { Constants.FAKE_RPC_SERVICE_PROVIDER,serviceRegistration.ServiceProvider },
                    { Constants.FAKE_RPC_SERVICE_INTERFACE, serviceRegistration.ServiceInterface },
                    { Constants.FAKE_RPC_SERVICE_PROTOCOLS, serviceRegistration.ServiceProtocols }
                }
            };

            if (serviceRegistration.ExtraData != null && serviceRegistration.ExtraData.Any())
            {
                foreach (var item in serviceRegistration.ExtraData)
                    instance.Metadata[item.Key] = item.Value;
            }

            AsyncHelper.RunSync(() => _nacosNamingService.RegisterInstance(serviceRegistration.ServiceName, serviceRegistration.ServiceGroup, instance));
            _logger.LogInformation($"[REGISTER-SERVICE]  register {JsonConvert.SerializeObject(serviceRegistration)} to Nacos ...");
        }

        public override void Unregister(ServiceRegistration serviceRegistration)
        {
            var instance = new Instance()
            {
                Ip = serviceRegistration.ServiceHost,
                Port = serviceRegistration.ServicePort,
                ServiceName = serviceRegistration.ServiceName,
                ClusterName = $"{serviceRegistration.ServiceHost}:{serviceRegistration.ServicePort}",
                InstanceId = serviceRegistration.GetServiceId(),
                Ephemeral = false,
                Healthy = true,
                Enabled = true,
                Metadata = new Dictionary<string, string>()
                {
                    { Constants.FAKE_RPC_SERVICE_GROUP, serviceRegistration.ServiceGroup },
                    { Constants.FAKE_RPC_SERVICE_SCHEMA,serviceRegistration.ServiceSchema }
                }
            };

            if (serviceRegistration.ExtraData != null && serviceRegistration.ExtraData.Any())
            {
                foreach (var item in serviceRegistration.ExtraData)
                    instance.Metadata[item.Key] = item.Value;
            }

            AsyncHelper.RunSync(() => _nacosNamingService.DeregisterInstance(serviceRegistration.ServiceName, serviceRegistration.ServiceGroup, instance));
            _logger.LogInformation($"[UNREGISTER-SERVICE]  register {JsonConvert.SerializeObject(serviceRegistration)} from Nacos ...");
        }
    }
}
