using FakeRpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FakeRpc.Core.Mics;
using Newtonsoft.Json;
using FakeRpc.Core.Serialize;
using FakeRpc.Core.Invokers.WebSockets;

namespace FakeRpc.Server.Middlewares
{
    public class FakeRpcWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IWebSocketCallInvoker _callInvoker;

        private readonly ILogger<FakeRpcWebSocketMiddleware> _logger;

        public FakeRpcWebSocketMiddleware(RequestDelegate next, IWebSocketCallInvoker callInvoker, ILogger<FakeRpcWebSocketMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _callInvoker = callInvoker;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var connectionId = context.Connection.Id;
            var contentType = context.Request.Query["Content-Type"][0] ?? FakeRpcMediaTypes.Default;
            var serializationHandler = MessageSerializerFactory.Create(contentType);

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();

            _logger.LogInformation("Handle WebSocket Connection, ConnectionId: {0}, Content-Type: {1}...", connectionId, contentType);

            await HandleWebSocket(webSocket, serializationHandler);

        }

        private async Task HandleWebSocket(WebSocket webSocket, IMessageSerializer serializationHandler)
        {
            while (webSocket.State == WebSocketState.Open)
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

                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    case WebSocketMessageType.Binary:
                        // Protobuf 要求 byte[] 末尾不能有垃圾
                        var bytes = new byte[receivedLength];
                        Array.Copy(buffer, bytes, receivedLength);
                        var request = await serializationHandler.DeserializeAsync<FakeRpcRequest>(bytes);
                        await _callInvoker.Invoke(request, webSocket, serializationHandler);
                        break;
                }
            }
        }
    }
}
