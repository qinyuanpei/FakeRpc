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

        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<FakeRpcWebSocketMiddleware> _logger;

        public FakeRpcWebSocketMiddleware(IServiceProvider serviceProvider, RequestDelegate next, ILogger<FakeRpcWebSocketMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            // MessageSerializer
            var connectionId = context.Connection.Id;
            var contentType = context.Request.Query["Content-Type"][0] ?? FakeRpcMediaTypes.Default;
            var messageSerializer = MessageSerializerFactory.Create(contentType);

            // CallInvoker
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var callInvoker = new ServerWebSocketCallInvoker(_serviceProvider, webSocket, messageSerializer);

            _logger.LogInformation("Handle WebSocket Connection, ConnectionId: {0}, Content-Type: {1}...", connectionId, contentType);

            await HandleWebSocket(webSocket, messageSerializer, callInvoker);

        }

        private async Task HandleWebSocket(WebSocket webSocket, IMessageSerializer messageSerializer, IWebSocketCallInvoker callInvoker)
        {
            while (webSocket.State == WebSocketState.Open)
            {               
                var buffer = new byte[Constants.FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE];

                var receivedLength = 0;
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

                var bufferMemory = buffer.AsMemory();
                switch (receiveResult.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    case WebSocketMessageType.Binary:
                        // Protobuf 要求 byte[] 末尾不能有垃圾
                        var bytes = bufferMemory.Slice(0, receivedLength).ToArray();
                        var request = await messageSerializer.DeserializeAsync<FakeRpcRequest>(bytes);
                        await callInvoker.InvokeAsync(request);
                        break;
                }
            }
        }
    }
}
