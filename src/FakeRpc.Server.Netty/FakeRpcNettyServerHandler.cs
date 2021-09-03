using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using FakeRpc.Core.Mics;

namespace FakeRpc.Server.Netty
{
    public class FakeRpcNettyServerHandler : SimpleChannelInboundHandler<FakeRpcNettyRequest>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<FakeRpcServerOptions> _serverOptions;
        private readonly ILogger<FakeRpcNettyServerHandler> _logger;
        public FakeRpcNettyServerHandler(
            IServiceProvider serviceProvider,
            ILogger<FakeRpcNettyServerHandler> logger
        )
        {
            _serviceProvider = serviceProvider;
            _serverOptions = _serviceProvider.GetService<IOptions<FakeRpcServerOptions>>();
            _logger = logger;
        }

        protected override void ChannelRead0(IChannelHandlerContext context, FakeRpcNettyRequest rpcRequest)
        {
            var rpcResponse = new FakeRpcNettyResponse();
            rpcResponse.RequestId = rpcRequest.RequestId;
            try
            {
                var serviceType = _serverOptions.Value.ServiceTypes.FirstOrDefault(x => x.GetServiceName() == rpcRequest.ServiceName);
                if (serviceType == null)
                    throw new ArgumentException($"Service {rpcRequest.ServiceName} can't be resolved.");

                var serviceInstance = _serviceProvider.GetService(serviceType);
                var serviceMethod = serviceType.GetMethod(rpcRequest.MethodName);
                if (serviceMethod == null)
                    throw new ArgumentException($"Route {rpcRequest.ServiceName}/{rpcRequest.MethodName} can't be resolved.");

                var result = serviceMethod.Invoke(serviceInstance, rpcRequest.Parameters);
                rpcResponse.Result = result;
            }
            catch (Exception ex)
            {
                rpcResponse.Message = ex.Message;
                _logger.LogError("Invoke Rpc {0}/{1} fails dut to {2}", rpcRequest.ServiceName, rpcRequest.MethodName, ex.Message);
            };

            context.WriteAsync(rpcResponse);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }
    }
}
