using FakeRpc.Core.Mics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public class ServerStreamingCallInvoker : IServerStreamingCallInvoker
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly byte[] _buffer = new byte[1024];

        private readonly ILogger<ServerStreamingCallInvoker> _logger;

        public ServerStreamingCallInvoker(IServiceProvider serviceProvider, ILogger<ServerStreamingCallInvoker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(Stream stream)
        {
            var tcpFrame = await ReadFrame<FakeRpcRequest>(stream);

            var request = tcpFrame.Body;
            var response = new FakeRpcResponse() { Id = request.Id };

            var contentType = tcpFrame.Header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE];
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var serverOptions = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>()?.Value;

                // Locate Service
                var serviceDescriptor = serverOptions.ServiceDescriptors.FirstOrDefault(x => x.ServiceType.GetServiceName() == request.ServiceName && x.ServiceType.GetServiceGroup() == request.ServiceGroup);
                if (serviceDescriptor == null)
                {
                    response.Error = $"Please make sure the service \"{request.ServiceGroup}.{request.ServiceName}\" is registered.";
                    _logger?.LogError(response.Error);
                    await WriteFrame(stream, response, contentType);
                    return;
                }

                // Locate Method
                _logger.LogInformation("The type of service is {}.", serviceDescriptor.ServiceType.FullName);
                var serviceInstance = scope.ServiceProvider.GetRequiredService(serviceDescriptor.ServiceType);
                var serviceMethod = serviceInstance.GetType().GetMethod(request.MethodName);
                if (serviceMethod == null)
                {
                    response.Error = $"Please make sure the method \"{request.MethodName}\" is defined in the service \"{request.ServiceGroup}.{request.ServiceName}\".";
                    _logger?.LogError(response.Error);
                    await WriteFrame(stream, response, contentType);
                    return;
                }

                // Invoke Method
                if (serviceMethod.GetParameters().Length > 0)
                {
                    var parameterType = serviceMethod.GetParameters()[0].ParameterType;
                    var keyValueParams = JsonConvert.DeserializeObject<KeyValuePair<string, object>[]>(request.MethodParams);
                    var jsonfiyParams = JsonConvert.SerializeObject(keyValueParams[0].Value);
                    var methodParams = JsonConvert.DeserializeObject(jsonfiyParams, parameterType);
                    dynamic ret = serviceMethod.Invoke(serviceInstance, new object[] { methodParams });
                    response.SetResult(ret.Result);
                    _logger?.LogInformation("Invoke {0}/{1}, Parameters:{2}, Returns:{3}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, jsonfiyParams, JsonConvert.SerializeObject((object)ret.Result));
                }
                else
                {
                    dynamic ret = serviceMethod.Invoke(serviceInstance, null);
                    response.SetResult(ret.Result);
                    _logger?.LogInformation("Invoke {0}/{1}, Parameters: null, Returns:{2}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, JsonConvert.SerializeObject((object)ret.Result));
                }

                await WriteFrame(stream, response, contentType);
            }
        }

        private async Task<FakeRpcTcpFrame<TBody>> ReadFrame<TBody>(Stream stream) where TBody : class
        {
            var receivedLength = 0;
            var receivedBytes = new List<byte>();

            while (true)
            {
                var length = await stream.ReadAsync(_buffer, receivedLength, _buffer.Length);
                if (length == 0) break;
                receivedBytes.AddRange(_buffer.AsMemory().Slice(0, length).ToArray());
                receivedLength += length;
            }

            return FakeRpcTcpFrame.Decode<TBody>(receivedBytes.ToArray());
        }

        private async Task WriteFrame<TBody>(Stream stream, TBody body, string contentType) where TBody : class
        {
            var tcpFrame = new FakeRpcTcpFrame<TBody>();
            tcpFrame.Body = body;
            tcpFrame.Header = new Dictionary<string, string>
            {
                { Constants.FAKE_RPC_HEADER_CONTENT_TYPE, contentType }
            };

            var bytes = FakeRpcTcpFrame.Encode<TBody>(tcpFrame, contentType);

            stream.Position = 0;
            await stream.FlushAsync();
            await stream.WriteAsync(bytes);
        }
    }
}
