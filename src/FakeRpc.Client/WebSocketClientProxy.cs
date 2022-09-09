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
            dynamic result = null;

            // Prepare Request
            var request = FakeRpcRequest.Create(typeof(T));
            request.MethodName = targetMethod.Name;

            CallInvoker.OnMessageReceived += (sender, response) =>
            {
                // Resolve the type of return value
                var returnType = targetMethod.ReturnType;
                if (returnType.IsGenericType)
                    returnType = returnType.GetGenericArguments()[0];

                if (response.Id != request.Id) return;
                result = JsonConvert.DeserializeObject(response.Result, returnType);
            };

            CallInvoker.OnClosed += () =>
            {
                //Console.WriteLine("ClientWebSocket is Closed. Prepare  to connect it again.");
                CallInvoker.ConnectAsync(WebSocket, Uri, CancellationToken.None);
            };

            CallInvoker.OnConnecting += () =>
            {
                //Console.WriteLine("WebSocket Connecting...");
                Task.Delay(1);
            };

            CallInvoker.OnOpened += () =>
            {
                //Console.WriteLine("WebSocket Opened.");
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

            InternalInvoke(request);
            while (result == null) { Task.Delay(1); }
            return Task.FromResult(result);
        }

        private void InternalInvoke(FakeRpcRequest request)
        {
            CallInvoker?.InvokeAsync(request);
        }

        public void Dispose()
        {
            WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            WebSocket?.Dispose();
        }
    }
}
