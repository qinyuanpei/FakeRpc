using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Server.WebSockets
{
    public class ServerRpcBinder : ISocketRpcBinder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ServerRpcBinder> _logger;

        public Action<FakeRpcRequest> OnSend { get; set; }

        public Action<FakeRpcResponse<dynamic>> OnReceive { get; set; }

        public ServerRpcBinder(IServiceProvider serviceProvider, ILogger<ServerRpcBinder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Invoke(FakeRpcRequest request, WebSocket webSocket)
        {
            var response = new FakeRpcResponse<dynamic>() { Id = request.Id };

            using (var scope = _serviceProvider.CreateScope())
            {
                var serverOptions = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>()?.Value;

                // Locate Service
                var serviceDescriptor = serverOptions.ServiceDescriptors.FirstOrDefault(x => x.ServiceType.GetServiceName() == request.ServiceName && x.ServiceType.GetServiceGroup() == request.ServiceGroup);
                if (serviceDescriptor == null)
                {
                    response.Error = $"Please make sure the service \"{request.ServiceGroup}.{request.ServiceName}\" is registered.";
                    _logger.LogError(response.Error);
                    await SendMessage(response, webSocket);
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
                    await SendMessage(response, webSocket);
                    return;
                }

                // Invoke Method
                if (serviceMethod.GetParameters().Length > 0)
                {
                    var parameterType = serviceMethod.GetParameters()[0].ParameterType;
                    var jsonfiyParams = JsonConvert.SerializeObject(request.MethodParams[0].Value);
                    var methodParams = JsonConvert.DeserializeObject(jsonfiyParams, parameterType);
                    dynamic ret = serviceMethod.Invoke(serviceInstance, new object[] { methodParams });
                    response.Result = ret.Result;
                    _logger.LogInformation("Invoke {0}/{1}, Parameters:{2}, Returns:{3}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, jsonfiyParams, JsonConvert.SerializeObject((object)ret.Result));
                }
                else
                {
                    dynamic ret = serviceMethod.Invoke(serviceInstance, null);
                    response.Result = ret.Result;
                    _logger.LogInformation("Invoke {0}/{1}, Parameters: null, Returns:{2}", serviceDescriptor.ServiceType.GetServiceName(), serviceMethod.Name, JsonConvert.SerializeObject((object)ret.Result));
                }

                await SendMessage(response, webSocket);
            }

        }

        private async Task SendMessage(FakeRpcResponse<dynamic> response, WebSocket webSocket)
        {
            var jsonify = JsonConvert.SerializeObject(response);
            var payload = Encoding.UTF8.GetBytes(jsonify);
            await webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);
  
        }
    }
}
