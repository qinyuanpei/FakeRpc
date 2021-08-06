using FakeRpc.Client;
using FakeRpc.Core.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FakeRpc.ServiceRegistry.Redis
{
    public static class FakeRpcClientBuilderExtensions
    {
        public static FakeRpcClientBuilder EnableRedisServiceDiscovery(this FakeRpcClientBuilder builder, Action<RedisServiceDiscoveryOptions> setupAction)
        {
            var options = new RedisServiceDiscoveryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IServiceDiscovery, RedisServiceDiscovery>();
            return builder;
        }
    }

}