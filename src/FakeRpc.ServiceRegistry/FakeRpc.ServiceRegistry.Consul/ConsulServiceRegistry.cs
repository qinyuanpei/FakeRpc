using Consul;
using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.ServiceRegistry.Consul
{
    public class ConsulServiceRegistry : BaseServiceRegistry
    {
        private readonly IConsulClient _consulClient;
        private readonly ConsulServiceRegistryOptions _options;
        private readonly ILogger<ConsulServiceRegistry> _logger;
        public ConsulServiceRegistry(ConsulServiceRegistryOptions options, ILogger<ConsulServiceRegistry> logger)
        {
            _options = options;
            _consulClient = new ConsulClient(new ConsulClientConfiguration() { Address = new Uri(options.BaseUrl) });
            _logger = logger;
        }

        public override void Register(ServiceRegistration serviceRegistration)
        {
            var registerID = serviceRegistration.GetServiceId();

            AsyncHelper.RunSync<WriteResult>(() => _consulClient.Agent.ServiceDeregister(registerID));

            var agentServiceRegistration = new AgentServiceRegistration()
            {
                ID = registerID,
                Name = serviceRegistration.ServiceName,
                Address = serviceRegistration.ServiceHost,
                Port = serviceRegistration.ServicePort,
                Check = new AgentServiceCheck
                {
                    TCP = $"{serviceRegistration.ServiceHost}:{serviceRegistration.ServicePort}",
                    Status = HealthStatus.Passing,
                    TLSSkipVerify = true,
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(10),
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5)
                },
                Meta = new Dictionary<string, string>()
                {
                    { Constants.FAKE_RPC_SERVICE_GROUP, serviceRegistration.ServiceGroup },
                    { Constants.FAKE_RPC_SERVICE_SCHEMA,serviceRegistration.ServiceSchema },
                    { Constants.FAKE_RPC_SERVICE_PROVIDER,serviceRegistration.ServiceProvider },
                    { Constants.FAKE_RPC_SERVICE_INTERFACE, serviceRegistration.ServiceInterface },
                    { Constants.FAKE_RPC_SERVICE_PROTOCOLS, serviceRegistration.ServiceProtocols }
                }
            };

            if (serviceRegistration.ExtraData!= null && serviceRegistration.ExtraData.Any())
            {
                foreach (var item in serviceRegistration.ExtraData)
                    agentServiceRegistration.Meta[item.Key] = item.Value;
            }

            AsyncHelper.RunSync<WriteResult>(() => _consulClient.Agent.ServiceRegister(agentServiceRegistration));
            _logger.LogInformation($"[REGISTER-SERVICE]  register {JsonConvert.SerializeObject(serviceRegistration)} to Consul ...");
        }

        public override void Unregister(ServiceRegistration serviceRegistration)
        {
            var registerID = serviceRegistration.GetServiceId();
            AsyncHelper.RunSync<WriteResult>(() => _consulClient.Agent.ServiceDeregister(registerID));
            _logger.LogInformation($"[UNREGISTER-SERVICE]  unregister {JsonConvert.SerializeObject(serviceRegistration)} from Consul ...");
        }
    }
}
