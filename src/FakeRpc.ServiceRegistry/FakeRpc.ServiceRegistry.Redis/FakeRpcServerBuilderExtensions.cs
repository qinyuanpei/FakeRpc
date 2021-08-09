using FakeRpc.Core;
using FakeRpc.Core.Registry;
using FakeRpc.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FakeRpc.ServiceRegistry.Redis
{
    public static class FakeRpcServerBuilderExtensions
    {
        public static FakeRpcServerBuilder EnableRedisServiceRegistry(this FakeRpcServerBuilder builder, Action<RedisServiceRegistryOptions> setupAction)
        {
            var options = new RedisServiceRegistryOptions();
            setupAction?.Invoke(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IServiceRegistry, RedisServiceRegistry>();
            builder.Services.AddHostedService<RedisHeartBeatCheckService>();
            return builder;
        }
    }
}
