using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using FakeRpc.Core.Serialize;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using FakeRpc.Core.Mics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public class ServerWebSocketCallInvoker : IWebSocketCallInvoker
    {

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ServerWebSocketCallInvoker> _logger;

        public ServerWebSocketCallInvoker(IServiceProvider serviceProvider, ILogger<ServerWebSocketCallInvoker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Action<FakeRpcRequest> OnSend { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Action<FakeRpcResponse> OnReceive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task Invoke(FakeRpcRequest request, WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            var response = new FakeRpcResponse() { Id = request.Id };

            using (var scope = _serviceProvider.CreateScope())
            {
                var serverOptions = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>()?.Value;

                // Locate Service
                var serviceDescriptor = serverOptions.ServiceDescriptors.FirstOrDefault(x => x.ServiceType.GetServiceName() == request.ServiceName && x.ServiceType.GetServiceGroup() == request.ServiceGroup);
                if (serviceDescriptor == null)
                {
                    response.Error = $"Please make sure the service \"{request.ServiceGroup}.{request.ServiceName}\" is registered.";
                    _logger.LogError(response.Error);
                    await SendMessage(response, webSocket, serializationHandler);
                    return;
                }

                // Locate Method
                _logger.LogInformation("The type of service is {}.", serviceDescriptor.ServiceType.FullName);
                var serviceInstance = scope.ServiceProvider.GetRequiredService(serviceDescriptor.ServiceType);
                var serviceMethod = serviceInstance.GetType().GetMethod(request.MethodName);
                if (serviceMethod == null)
                {
                    response.Error = $"Please make sure the method \"{request.MethodName}\" is defined in the service \"{request.ServiceGroup}.{request.ServiceName}\".";
                    _logger.LogError(response.Error);
                    await SendMessage(response, webSocket, serializationHandler);
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
                    _logger.LogInformation("Invoke {0}/{1}, Parameters:{2}, Returns:{3}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, jsonfiyParams, JsonConvert.SerializeObject((object)ret.Result));
                }
                else
                {
                    dynamic ret = serviceMethod.Invoke(serviceInstance, null);
                    response.SetResult(ret.Result);
                    _logger.LogInformation("Invoke {0}/{1}, Parameters: null, Returns:{2}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, JsonConvert.SerializeObject((object)ret.Result));
                }

                await SendMessage(response, webSocket, serializationHandler);
            }

        }

        private async Task SendMessage(FakeRpcResponse response, WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            var payload = serializationHandler.Serialize(response);
            await webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);

        }
    }
}
