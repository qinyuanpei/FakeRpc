using FakeRpc.Core;
using FakeRpc.Core.Registry;
using FakeRpc.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.ServiceRegistry.Consul
{
    public static class FakeRpcServerBuilderExtensions
    {
        public static FakeRpcServerBuilder EnableConsulServiceRegistry(this FakeRpcServerBuilder builder, Action<ConsulServiceRegistryOptions> setupAction)
        {
            var options = new ConsulServiceRegistryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IServiceRegistry, ConsulServiceRegistry>();
            return builder;
        }
    }
}
