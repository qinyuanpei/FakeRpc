using Consul;
using FakeRpc.Core.Mics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.ServiceDiscovery.Consul
{
    public static class AgentServiceExtensions
    {
        public static string GetServiceGroup(this AgentService agentService)
        {
            if (agentService.Meta == null | !agentService.Meta.Any())
                return string.Empty;

            if (!agentService.Meta.ContainsKey(Constants.FAKE_RPC_SERVICE_GROUP))
                return string.Empty;

            return agentService.Meta[Constants.FAKE_RPC_SERVICE_GROUP];
        }

        public static string GetServiceSchema(this AgentService agentService)
        {
            if (agentService.Meta == null | !agentService.Meta.Any())
                return Constants.FAKE_RPC_SERVICE_SCHEMA_HTTP;

            if (!agentService.Meta.ContainsKey(Constants.FAKE_RPC_SERVICE_SCHEMA))
                return Constants.FAKE_RPC_SERVICE_SCHEMA_HTTP;

            return agentService.Meta[Constants.FAKE_RPC_SERVICE_SCHEMA];
        }
    }
}
