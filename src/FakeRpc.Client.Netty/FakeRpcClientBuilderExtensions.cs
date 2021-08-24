using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Client.Netty
{
    public static class FakeRpcClientBuilderExtensions
    {
        public static FakeRpcClientBuilder AddNetty(this FakeRpcClientBuilder builder)
        {
            builder.Services.AddSingleton<FakeRpcNettyClientHost>();
            builder.Services.AddTransient<FakeRpcNettyClientHandler>();

            return builder;
        }

        public static async Task UseNetty(this FakeRpcClientBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var nettyClientHost = serviceProvider.GetService<FakeRpcNettyClientHost>();
            await nettyClientHost.RunAsync();
        }
    }
}
