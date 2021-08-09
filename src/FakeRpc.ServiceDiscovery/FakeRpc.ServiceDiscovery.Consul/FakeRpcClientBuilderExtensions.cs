using FakeRpc.Client;
using FakeRpc.Core.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.ServiceDiscovery.Consul
{
    public static class FakeRpcClientBuilderExtensions
    {
        public static FakeRpcClientBuilder EnableConsulServiceDiscovery(this FakeRpcClientBuilder builder, Action<ConsulServiceDiscoveryOptions> setupAction)
        {
            var options = new ConsulServiceDiscoveryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
            return builder;
        }
    }
}
