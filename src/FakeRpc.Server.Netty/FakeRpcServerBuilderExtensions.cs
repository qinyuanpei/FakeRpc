using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Server.Netty
{
    public static class FakeRpcServerBuilderExtensions
    {
        public static FakeRpcServerBuilder AddNetty(this FakeRpcServerBuilder builder)
        {
            builder.Services.AddSingleton<FakeRpcNettyServerHost>();
            builder.Services.AddTransient<IChannelHandler, FakeRpcNettyServerHandler>();

            return builder;
        }

        public static async Task UseNetty(this FakeRpcServerBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var nettyServerHost = serviceProvider.GetService<FakeRpcNettyServerHost>();
            await nettyServerHost.RunAsync();
        }
    }
}
