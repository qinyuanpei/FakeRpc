using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public class ClientWebSocketCallInvoker : IWebSocketCallInvoker
    {
        private readonly WebSocket _webSocket;

        private IMessageSerializer _messageSerializer;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ClientWebSocketCallInvoker>? _logger;

        private readonly Channel<ArraySegment<byte>> _messagesToSendQueue =
            Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

        public EventHandler<FakeRpcRequest> OnMessageSent { get; set; }
        public EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }
        public Action OnConnecting { get; set; }
        public Action OnOpened { get; set; }
        public Action OnClosed { get; set; }

        public ClientWebSocketCallInvoker(IServiceProvider serviceProvider, WebSocket webSocket, IMessageSerializer messageSerializer)
        {
            _webSocket = webSocket;
            _serviceProvider = serviceProvider;
            _messageSerializer = messageSerializer;
            _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<ClientWebSocketCallInvoker>();
        }

        public async Task InvokeAsync(FakeRpcRequest request)
        {
            while (_webSocket.State == WebSocketState.Closed) { OnClosed?.Invoke(); }
            while (_webSocket.State == WebSocketState.Connecting) { OnConnecting?.Invoke(); }
            if (_webSocket.State == WebSocketState.Open) { OnOpened?.Invoke(); }

            await SendMessage(request);
            await ReceiveMessage(request);

        }

        private async Task SendMessage(FakeRpcRequest request)
        {
            var payload = await  _messageSerializer.SerializeAsync<FakeRpcRequest>(request);
            await _webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            _logger?.LogInformation("Send request to {0}/{1}, Parameters:{3}", request.ServiceName, request.MethodName, request.MethodParams);
            OnMessageSent?.Invoke(_webSocket, request);
        }

        private async Task ReceiveMessage(FakeRpcRequest request)
        {
            var receivedLength = 0;
            byte[] buffer = new byte[Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE];
            WebSocketReceiveResult receiveResult = null;

            do
            {
                receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                receivedLength += receiveResult.Count;

                if (receivedLength >= Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE)
                {
                    var statusDescription = string.Format(Constants.FAKE_RPC_WEBSOCKET_MESSAGE_TOO_BIG, receivedLength, Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE);
                    await _webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, statusDescription, CancellationToken.None);
                    break;
                }
            }
            while (receiveResult?.EndOfMessage == false);

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                return;
            }

            // Protobuf 要求 byte[] 末尾不能有垃圾
            var bufferMemory = buffer.AsMemory();
            var bytes = bufferMemory.Slice(0, receivedLength).ToArray();
            var response = await _messageSerializer.DeserializeAsync<FakeRpcResponse>(bytes);
            _logger?.LogInformation("Receive response of {0}/{1}, Payload:{2}", request.ServiceName, request.MethodName, response.Result);
            OnMessageReceived?.Invoke(_webSocket, response);
        }

        public Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token)
        {
            _logger?.LogInformation("Connect to {0}", uri.AbsolutePath);
            return (webSocket as ClientWebSocket).ConnectAsync(uri, token);
        }
    }
}
