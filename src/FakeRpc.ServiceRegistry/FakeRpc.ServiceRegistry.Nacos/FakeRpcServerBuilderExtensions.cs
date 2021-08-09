using FakeRpc.Core;
using FakeRpc.Core.Registry;
using FakeRpc.Server;
using FakeRpc.ServiceRegistry.Nacos;
using Microsoft.Extensions.DependencyInjection;
using Nacos.V2.DependencyInjection;
using System;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public static class FakeRpcServerBuilderExtensions
    {
        public static FakeRpcServerBuilder EnableNacosServiceRegistry(this FakeRpcServerBuilder builder, Action<NacosServiceRegistryOptions> setupAction)
        {
            var options = new NacosServiceRegistryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddNacosV2Naming(opt =>
            {
                opt.ServerAddresses = options.ServerAddress;
                opt.EndPoint = string.Empty;
                opt.Namespace = options.Namespace;
                opt.NamingUseRpc = false;
            });
            builder.Services.AddSingleton<IServiceRegistry, NacosServiceRegistry>();
            return builder;
        }
    }
}
