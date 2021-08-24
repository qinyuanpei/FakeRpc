using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;

namespace FakeRpc.Server.Netty
{
    public class FakeRpcNettyServerHandler : ChannelHandlerAdapter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FakeRpcNettyServerHandler> _logger;
        public FakeRpcNettyServerHandler(
            IServiceProvider serviceProvider,
            ILogger<FakeRpcNettyServerHandler> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            context.WriteAsync(byteBuffer);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }
    }
}
