using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace FakeRpc.Server.Netty
{
    public class FakeRpcNettyServerHost
    {
        private readonly IServiceProvider _serviceProvider;
        public FakeRpcNettyServerHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task RunAsync()
        {
            var eventLoopGroup = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new ServerBootstrap()
                    .Group(eventLoopGroup)
                    .Channel<TcpServerSocketChannel>()
                    .ChildOption(ChannelOption.SoKeepalive, true)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        var pipeline = channel.Pipeline;
                        var handlers = _serviceProvider.GetService<IEnumerable<IChannelHandler>>();
                        handlers.ToList().ForEach(handler => pipeline.AddLast(handler));
                    }));


                var boundChannel = await bootstrap.BindAsync(3000);
                Console.WriteLine("FakeRpc Netty Server is listening on 3000...");
                Console.ReadLine();
                await boundChannel.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await eventLoopGroup.ShutdownGracefullyAsync();
            }
        }
    }
}
