using FakeRpc.Core.Mics;
using Nacos.V2.Naming.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.ServiceDiscovery.Nacos
{
    public static class InstanceExtensions
    {
        public static string GetServiceSchema(this Instance instance)
        {
            if (instance.Metadata == null | !instance.Metadata.Any())
                return Constants.FAKE_RPC_SERVICE_SCHEMA_HTTP;

            if (!instance.Metadata.ContainsKey(Constants.FAKE_RPC_SERVICE_SCHEMA))
                return Constants.FAKE_RPC_SERVICE_SCHEMA_HTTP;

            return instance.Metadata[Constants.FAKE_RPC_SERVICE_SCHEMA];
        }
    }
}
