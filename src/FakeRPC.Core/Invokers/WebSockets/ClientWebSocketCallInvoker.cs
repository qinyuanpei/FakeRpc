using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.WebSockets
{
    public class ClientWebSocketCallInvoker : IWebSocketCallInvoker
    {
        private readonly ILogger _logger;

        public Action<FakeRpcRequest> OnSend { get; set; }

        public Action<FakeRpcResponse> OnReceive { get; set; }

        public ClientWebSocketCallInvoker(ILogger<ClientWebSocketCallInvoker> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FakeRpcRequest request, WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            await SendMessage(request, webSocket, serializationHandler);
            await ReceiveMessage(request, webSocket, serializationHandler);

        }

        private async Task SendMessage(FakeRpcRequest request, WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            var payload = await serializationHandler.SerializeAsync<FakeRpcRequest>(request);
            _logger.LogInformation("{0}", JsonConvert.SerializeObject(request));
            await webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            _logger.LogInformation("Send request to {0}/{1}, Parameters:{3}", request.ServiceName, request.MethodName, request.MethodParams);
            OnSend?.Invoke(request);
        }

        private async Task ReceiveMessage(FakeRpcRequest request, WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            var receivedLength = 0;
            byte[] buffer = new byte[Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE];
            WebSocketReceiveResult receiveResult = null;

            do
            {
                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                receivedLength += receiveResult.Count;

                if (receivedLength >= Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE)
                {
                    var statusDescription = string.Format(Constants.FAKE_RPC_WEBSOCKET_MESSAGE_TOO_BIG, receivedLength, Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE);
                    await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, statusDescription, CancellationToken.None);
                    break;
                }
            }
            while (receiveResult?.EndOfMessage == false);

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                return;
            }

            // Protobuf 要求 byte[] 末尾不能有垃圾
            var bytes = new byte[receivedLength];
            Array.Copy(buffer, bytes, receivedLength);
            var response = await serializationHandler.DeserializeAsync<FakeRpcResponse>(bytes);
            _logger.LogInformation("Receive response of {0}/{1}, Payload:{2}", request.ServiceName, request.MethodName, response.Result);
            OnReceive?.Invoke(response);
        }
    }
}
