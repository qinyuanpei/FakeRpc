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
using System.Threading.Channels;
using System.IO;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public class ServerWebSocketCallInvoker : IServerWebSocketCallInvoker
    {

        private readonly WebSocket _webSocket;

        private IMessageSerializer _messageSerializer;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ServerWebSocketCallInvoker>? _logger;

        private readonly Channel<ArraySegment<byte>> _messagesToSendQueue =
            Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

        public ServerWebSocketCallInvoker(IServiceProvider serviceProvider, WebSocket webSocket, IMessageSerializer messageSerializer)
        {
            _webSocket = webSocket;
            _serviceProvider = serviceProvider;
            _messageSerializer = messageSerializer;
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger<ServerWebSocketCallInvoker>();
            ListenMessageQueue();
        }

        public EventHandler<FakeRpcRequest> OnMessageSent { get; set; }
        public EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }
        public Action OnConnecting { get; set; }
        public Action OnOpened { get; set; }
        public Action OnClosed { get; set; }

        public async Task InvokeAsync(Stream stream)
        {
            var request = _messageSerializer.Deserialize<FakeRpcRequest>((stream as MemoryStream).ToArray());
            var response = new FakeRpcResponse() { Id = request.Id };

            using (var scope = _serviceProvider.CreateScope())
            {
                var serverOptions = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>()?.Value;

                // Locate Service
                var serviceDescriptor = serverOptions.ServiceDescriptors.FirstOrDefault(x => x.ServiceType.GetServiceName() == request.ServiceName && x.ServiceType.GetServiceGroup() == request.ServiceGroup);
                if (serviceDescriptor == null)
                {
                    response.Error = $"Please make sure the service \"{request.ServiceGroup}.{request.ServiceName}\" is registered.";
                    _logger?.LogError(response.Error);
                    await EnqueueMessage(response);
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
                    await EnqueueMessage(response);
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

                await EnqueueMessage(response);
            }
        }

        public Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token)
        {
            return Task.CompletedTask;
        }
        private async Task SendMessageInternal(ArraySegment<byte> message)
        {
            await _webSocket.SendAsync(message, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        private async Task EnqueueMessage(FakeRpcResponse response)
        {
            var payload = await _messageSerializer.SerializeAsync(response);
            _messagesToSendQueue.Writer.TryWrite(new ArraySegment<byte>(payload));
        }

        private async Task SendMessagesFromQueue()
        {
            try
            {
                while (await _messagesToSendQueue.Reader.WaitToReadAsync())
                {
                    while (_messagesToSendQueue.Reader.TryRead(out var message))
                    {
                        try
                        {
                            await SendMessageInternal(message);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Failed to send message due to {e.Message}");
                        }
                    }
                }
            }
            catch (TaskCanceledException){}
            catch (OperationCanceledException){}
            catch (Exception e)
            {
                _logger.LogError(e, $"Restart listen message queue due to {e.Message}");
                ListenMessageQueue();
            }

        }

        private void ListenMessageQueue()
        {
            _ = Task.Factory.StartNew(_ => SendMessagesFromQueue(), TaskCreationOptions.LongRunning, CancellationToken.None);
        }
    }
}
