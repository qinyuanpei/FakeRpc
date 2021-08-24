using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Client.Netty
{
    public class FakeRpcNettyClientHandler : ChannelDuplexHandler
    {
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            base.ChannelRead(context, message);
        }
    }
}
