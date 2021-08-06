using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.ServiceRegistry.Redis
{
    public class RedisServiceRegistryOptions
    {
        public string RedisUrl { get; set; }
        public string RegisterEventTopic { get; set; } = "evt_service_register";
        public string UnregisterEventTopic { get; set; } = "evt_service_unregister";
    }
}
