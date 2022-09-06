using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Client.WebSockets
{
    public class WebSocketClientProxyBase : DispatchProxy
    {
        public Type ServiceType { get; set; }

        public WebSocket WebSocket { get; set; }

        public ISocketRpcBinder SocketRpcBinder { get; set; }

        public Uri Url { get; set; } 

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            while (WebSocket.State == WebSocketState.Closed)
            {
                (WebSocket as ClientWebSocket).ConnectAsync(Url, CancellationToken.None);
            }

            dynamic result = null;

            var request = FakeRpcRequest.Create(ServiceType);
            request.MethodName = targetMethod.Name;

            var returnType = targetMethod.ReturnType;
            if (returnType.IsGenericType)
                returnType = returnType.GetGenericArguments()[0];

            SocketRpcBinder.OnReceive += response =>
            {
                var jsonify = JsonConvert.SerializeObject(response.Result);
                result = JsonConvert.DeserializeObject(jsonify, returnType);
            };

            if (args.Length == 1)
            {
                // Unary Request Call
                var methodParam = new KeyValuePair<string, object>(targetMethod.GetParameters()[0].Name, args[0]);
                request.MethodParams = new KeyValuePair<string, object>[] { methodParam };
            }
            else if (args.Length == 0)
            {
                // Empty Request Call
                request.MethodParams = new KeyValuePair<string, object>[] { };
            }
            else
            {
                throw new Exception("FakeRpc only support a RPC method with 0 or 1 parameter");
            }

            SocketRpcBinder?.Invoke(request, WebSocket);
            while (result == null) { Thread.Sleep(1000); }
            return Task.FromResult(result);
        }
    }
}
