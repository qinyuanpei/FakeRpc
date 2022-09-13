using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public class ClientWebSocketCallInvoker : IClientWebSocketCallInvoker
    {
        private readonly WebSocket _webSocket;

        private IMessageSerializer _messageSerializer;

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<ClientWebSocketCallInvoker>? _logger;

        private readonly Channel<ArraySegment<byte>> _messagesToReadQueue =
            Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });

        public EventHandler<FakeRpcRequest> OnMessageSent { get; set; }
        public EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }
        public Action OnConnecting { get; set; }
        public Action OnOpened { get; set; }
        public Action<WebSocketClosedEventArgs> OnClosed { get; set; }

        public ClientWebSocketCallInvoker(IServiceProvider serviceProvider, WebSocket webSocket, IMessageSerializer messageSerializer)
        {
            _webSocket = webSocket;
            _serviceProvider = serviceProvider;
            _messageSerializer = messageSerializer;
            _logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<ClientWebSocketCallInvoker>();
            ListenMessageQueue();
        }

        public async Task InvokeAsync(FakeRpcRequest request)
        {
            while (_webSocket.State == WebSocketState.Closed) { OnClosed?.Invoke(new WebSocketClosedEventArgs() { CloseStatus = WebSocketCloseStatus.Empty, StatusDescription = string.Empty}); }
            while (_webSocket.State == WebSocketState.Connecting) { OnConnecting?.Invoke(); }
            if (_webSocket.State == WebSocketState.Open) { OnOpened?.Invoke(); }

            await SendMessage(request).ContinueWith(async _ => {
                await ReceiveMessage(request);
            });
        }

        private async Task SendMessage(FakeRpcRequest request)
        {
            var payload = await _messageSerializer.SerializeAsync<FakeRpcRequest>(request);
            await _webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            _logger?.LogInformation("Send request to {0}/{1}, parameters:{3}", request.ServiceName, request.MethodName, request.MethodParams);
            OnMessageSent?.Invoke(_webSocket, request);
        }

        private async Task ReceiveMessage(FakeRpcRequest request)
        {
            using (var stream = new MemoryStream())
            {
                var receivedLength = 0;
                var receivedBuffer = new byte[Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE];
                WebSocketReceiveResult receivedResult = null;

                do
                {
                    receivedResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivedBuffer), CancellationToken.None);
                    await stream.WriteAsync(receivedBuffer, receivedLength, receivedResult.Count);
                    receivedLength += receivedResult.Count;
                    if (receivedLength >= Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE)
                    {
                        var statusDescription = string.Format(Constants.FAKE_RPC_WEBSOCKET_MESSAGE_TOO_BIG, receivedLength, Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE);
                        await _webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, statusDescription, CancellationToken.None);
                        OnClosed?.Invoke(new WebSocketClosedEventArgs() { CloseStatus = WebSocketCloseStatus.MessageTooBig, StatusDescription = statusDescription });
                        return;
                    }
                }
                while (receivedResult?.EndOfMessage == false);

                switch (receivedResult?.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        OnClosed?.Invoke(new WebSocketClosedEventArgs() { CloseStatus = WebSocketCloseStatus.NormalClosure, StatusDescription = string.Empty });
                        break;
                    case WebSocketMessageType.Binary:
                        var bytes = stream.ToArray();
                        var response = await _messageSerializer.DeserializeAsync<FakeRpcResponse>(bytes);
                        _logger?.LogInformation("Send response to {0}/{1}, payload:{3}", request.ServiceName, request.MethodName, response.Result);
                        _messagesToReadQueue.Writer.TryWrite(new ArraySegment<byte>(bytes));
                        break;
                }
            }
        }

        public Task ConnectAsync(WebSocket webSocket, Uri uri, CancellationToken token)
        {
            _logger?.LogInformation("Connect to {0}", uri.AbsolutePath);
            return (webSocket as ClientWebSocket).ConnectAsync(uri, token);
        }

        private async Task ReadMessagesFromQueue()
        {
            try
            {
                while (await _messagesToReadQueue.Reader.WaitToReadAsync())
                {
                    while (_messagesToReadQueue.Reader.TryRead(out var message))
                    {
                        try
                        {
                            var response = await _messageSerializer.DeserializeAsync<FakeRpcResponse>(message.Array);
                            OnMessageReceived?.Invoke(_webSocket, response);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Failed to send message due to {e.Message}");
                        }
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _logger.LogError(e, $"Restart listen message queue due to {e.Message}");
                ListenMessageQueue();
            }

        }

        private void ListenMessageQueue()
        {
            _ = Task.Factory.StartNew(_ => ReadMessagesFromQueue(), TaskCreationOptions.LongRunning, CancellationToken.None);
        }
    }
}
