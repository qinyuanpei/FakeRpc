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
using FakeRpc.Core.WebSockets;

namespace FakeRpc.Server.Middlewares
{
    public class FakeRpcWebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ISocketRpcBinder _socketRpcBinder;

        private const int MAX_BUFFER_SIZE = 64 * 1024;

        private const string MESSAGE_TOO_BIG = "The message exceeds the maximum allowed message size: {0} of allowed {1} bytes.";

        private readonly ILogger<FakeRpcWebSocketMiddleware> _logger;

        public FakeRpcWebSocketMiddleware(RequestDelegate next, ISocketRpcBinder socketRpcBinder, ILogger<FakeRpcWebSocketMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _socketRpcBinder = socketRpcBinder;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocket(webSocket);
        }

        private async Task HandleWebSocket(WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var receivedLength = 0;
                byte[] buffer = new byte[MAX_BUFFER_SIZE];
                WebSocketReceiveResult receiveResult = null;

                do
                {
                    receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    receivedLength += receiveResult.Count;
                    if (receivedLength >= MAX_BUFFER_SIZE)
                    {
                        var statusDescription = string.Format(MESSAGE_TOO_BIG, receivedLength, MAX_BUFFER_SIZE);
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
                    case WebSocketMessageType.Text:
                        var message = Encoding.UTF8.GetString(buffer, 0, receivedLength);
                        await ProcessTextMessage(webSocket, message);
                        break;
                    case WebSocketMessageType.Binary:
                        await ProcessBinaryMessage(webSocket, new ArraySegment<byte>(buffer, 0, receivedLength));
                        break;
                }
            }
        }

        private async Task ProcessTextMessage(WebSocket webSocket, string message)
        {
            var request = FakeRpcRequest.Parse(message);
            await _socketRpcBinder.Invoke(request, webSocket);
        }

        private async Task ProcessBinaryMessage(WebSocket webSocket, ArraySegment<byte> message)
        {
            await Task.CompletedTask;
        }
    }
}
