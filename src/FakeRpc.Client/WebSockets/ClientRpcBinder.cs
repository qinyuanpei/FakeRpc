using FakeRpc.Core;
using FakeRpc.Core.WebSockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Client.WebSockets
{
    public class ClientRpcBinder : ISocketRpcBinder
    {
        private readonly ILogger _logger;

        private const int MAX_BUFFER_SIZE = 64 * 1024;

        private const string MESSAGE_TOO_BIG = "The message exceeds the maximum allowed message size: {0} of allowed {1} bytes.";

        public Action<FakeRpcRequest> OnSend { get; set; }

        public Action<FakeRpcResponse<dynamic>> OnReceive { get; set; }

        public ClientRpcBinder(ILogger<ClientRpcBinder> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FakeRpcRequest request, WebSocket webSocket)
        {
            await SendMessage(request, webSocket); ;
            await ReceiveMessage(webSocket);

        }

        private async Task SendMessage(FakeRpcRequest request, WebSocket webSocket)
        {
            var jsonify = JsonConvert.SerializeObject(request);
            var payload = Encoding.UTF8.GetBytes(jsonify);
            await webSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);
            _logger.LogInformation("Send RPC request {0}/{1}, Parameters:{3}", request.ServiceName, request.MethodName, jsonify);
            OnSend?.Invoke(request);
        }

        private async Task ReceiveMessage(WebSocket webSocket)
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

            var message = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            var response = JsonConvert.DeserializeObject<FakeRpcResponse<dynamic>>(message);
            _logger.LogInformation("Receive RPC response, Payload:{0}", message);
            OnReceive?.Invoke(response);
        }
    }
}
