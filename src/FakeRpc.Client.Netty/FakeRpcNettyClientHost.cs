using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FakeRpc.Client.Netty
{
    public class FakeRpcNettyClientHost
    {
        private readonly IServiceProvider _serviceProvider;

        public FakeRpcNettyClientHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync()
        {
            var envetLoopGroup = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new Bootstrap()
                    .Group(envetLoopGroup)
                    .Channel<TcpSocketChannel>()
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        var handler = _serviceProvider.GetService<FakeRpcNettyClientHandler>();
                        pipeline.AddLast(handler);
                    }));

                var clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("192.168.50.162"), 3000));
                Console.ReadLine();
                await clientChannel.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await envetLoopGroup.ShutdownGracefullyAsync();
            }
        }
    }
}
