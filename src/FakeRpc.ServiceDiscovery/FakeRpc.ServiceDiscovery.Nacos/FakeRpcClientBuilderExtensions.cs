using FakeRpc.Client;
using FakeRpc.Core.Discovery;
using FakeRpc.ServiceDiscovery.Nacos;
using Microsoft.Extensions.DependencyInjection;
using Nacos.V2.DependencyInjection;
using System;

namespace FakeRpc.ServiceRegistry.Nacos
{
    public static class FakeRpcClientBuilderExtensions
    {
        public static FakeRpcClientBuilder EnableNacosServiceDiscovery(this FakeRpcClientBuilder builder, Action<NacosServiceDiscoveryOptions> setupAction)
        {
            var options = new NacosServiceDiscoveryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddNacosV2Naming(opt =>
            {
                opt.ServerAddresses = options.ServerAddress;
                opt.EndPoint = string.Empty;
                opt.Namespace = options.Namespace;
                opt.NamingUseRpc = false;
            });
            builder.Services.AddSingleton<IServiceDiscovery, NacosServiceDiscovery>();
            return builder;
        }
    }

}