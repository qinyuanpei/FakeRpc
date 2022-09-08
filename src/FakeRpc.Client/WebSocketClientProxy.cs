using FakeRpc.Core;
using FakeRpc.Core.Invokers.WebSockets;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.Client
{
    public class WebSocketClientProxy<T> : DispatchProxy, IDisposable
    {
        public WebSocket WebSocket { get; set; }

        public IWebSocketCallInvoker CallInvoker { get; set; }

        public Uri Uri { get; set; }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            while (WebSocket.State == WebSocketState.Connecting) { Task.Delay(1); }

            while (WebSocket.State == WebSocketState.Closed)
            {
                (WebSocket as ClientWebSocket).ConnectAsync(Uri, CancellationToken.None);
            }

            dynamic result = null;

            var request = FakeRpcRequest.Create(typeof(T));
            request.MethodName = targetMethod.Name;

            var returnType = targetMethod.ReturnType;
            if (returnType.IsGenericType)
                returnType = returnType.GetGenericArguments()[0];

            CallInvoker.OnReceive += response =>
            {
                if (response.Id != request.Id) return;
                result = JsonConvert.DeserializeObject(response.Result, returnType);
            };

            if (args.Length == 1)
            {
                // Unary Request Call
                var methodParam = new KeyValuePair<string, object>(targetMethod.GetParameters()[0].Name, args[0]);
                request.MethodParams = JsonConvert.SerializeObject(new KeyValuePair<string, object>[] { methodParam });
            }
            else if (args.Length == 0)
            {
                // Empty Request Call
                request.MethodParams = JsonConvert.SerializeObject(new KeyValuePair<string, object>[] { });
            }
            else
            {
                throw new Exception("FakeRpc only support a RPC method with 0 or 1 parameter");
            }

            var contentType = FakeRpcMediaTypes.Default;
            if (Uri.GetQueryStrings().TryGetValue("Content-Type", out var value))
                contentType = value;

            var serializationHandler = MessageSerializerFactory.Create(contentType);
            CallInvoker?.Invoke(request, WebSocket, serializationHandler);
            while (result == null) { Task.Delay(1); }
            return Task.FromResult(result);
        }

        public void Dispose()
        {
            WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            WebSocket?.Dispose();
        }
    }
}
