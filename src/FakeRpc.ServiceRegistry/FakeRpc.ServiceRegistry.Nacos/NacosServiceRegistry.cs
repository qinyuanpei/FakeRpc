using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using Nacos;
using Nacos.V2;
using Nacos.V2.Naming.Dtos;
using System;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public class NacosServiceRegistry : BaseServiceRegistry
    {
        private readonly INacosNamingService _nacosNamingService;

        public NacosServiceRegistry(INacosNamingService nacosNamingService)
        {
            _nacosNamingService = nacosNamingService;
        }

        public override void Register(ServiceRegistration serviceRegistration)
        {
            var instance = BuildInstance(serviceRegistration);
            AsyncHelper.RunSync(() => _nacosNamingService.DeregisterInstance(serviceRegistration.ServiceName, serviceRegistration.ServiceGroup, instance));
            AsyncHelper.RunSync(() => _nacosNamingService.RegisterInstance(serviceRegistration.ServiceName,serviceRegistration.ServiceGroup, instance));
        }

        public override void Unregister(ServiceRegistration serviceRegistration)
        {
            var instance = BuildInstance(serviceRegistration);
            AsyncHelper.RunSync(() => _nacosNamingService.DeregisterInstance(serviceRegistration.ServiceName, serviceRegistration.ServiceGroup, instance));
        }

        private Instance BuildInstance(ServiceRegistration serviceRegistration)
        {
            var instance = new Instance();
            instance.Ip = serviceRegistration.ServiceUri.Host;
            instance.Port = serviceRegistration.ServiceUri.Port;
            instance.ServiceName = serviceRegistration.ServiceName;
            instance.Ephemeral = false;
            instance.Healthy = true;
            instance.Enabled = true;
            return instance;
        }
    }
}
