using FakeRpc.Client.WebSockets;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.Mics;
using FakeRpc.Core.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FakeRpc.Client
{
    public class FakeRpcClientFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FakeRpcClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public FakeRpcClientFactory()
        {
            _serviceProvider = new ServiceCollection().BuildServiceProvider();
        }

        public TClient Create<TClient>(Func<HttpClient, IFakeRpcCalls> rpcCallsFactory = null)
        {
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(typeof(TClient).GetClientName());
            if (rpcCallsFactory == null)
                rpcCallsFactory = _serviceProvider.GetService<Func<HttpClient, IFakeRpcCalls>>();

            var clientProxy = DispatchProxy.Create<TClient, ClientProxyBase>();
            (clientProxy as ClientProxyBase).HttpClient = httpClient;
            (clientProxy as ClientProxyBase).ServiceType = typeof(TClient);
            (clientProxy as ClientProxyBase).RpcCalls = rpcCallsFactory == null ?
                new DefaultFakeRpcCalls(httpClient) : rpcCallsFactory(httpClient);

            return clientProxy;
        }

        public TClient Create<TClient>(Uri baseUri, Func<HttpClient, IFakeRpcCalls> rpcCallsFactory = null)
        {
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var clientProxy = DispatchProxy.Create<TClient, ClientProxyBase>();
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = baseUri;
            if (rpcCallsFactory == null)
                rpcCallsFactory = _serviceProvider.GetService<Func<HttpClient, IFakeRpcCalls>>();

            (clientProxy as ClientProxyBase).HttpClient = httpClient;
            (clientProxy as ClientProxyBase).ServiceType = typeof(TClient);
            (clientProxy as ClientProxyBase).RpcCalls = rpcCallsFactory == null ?
                new DefaultFakeRpcCalls(httpClient) : rpcCallsFactory(httpClient);

            return clientProxy;
        }

        public TClient Create<TClient>(string baseUrl, Func<HttpClient, IFakeRpcCalls> rpcCallsFactory = null)
        {
            var baseUri = new Uri(baseUrl);
            return Create<TClient>(baseUri, rpcCallsFactory);
        }

        public TClient CreateSocketClient<TClient>(string baseUrl)
        {
            var clientProxy = DispatchProxy.Create<TClient, WebSocketClientProxyBase>();
            (clientProxy as WebSocketClientProxyBase).WebSocket = new ClientWebSocket();
            (clientProxy as WebSocketClientProxyBase).ServiceType = typeof(TClient);
            (clientProxy as WebSocketClientProxyBase).SocketRpcBinder = _serviceProvider.GetService<ISocketRpcBinder>();
            (clientProxy as WebSocketClientProxyBase).Url = new Uri(baseUrl);

            ((clientProxy as WebSocketClientProxyBase).WebSocket as ClientWebSocket).ConnectAsync(new Uri(baseUrl), CancellationToken.None);

            return clientProxy;
        }

        public TClient Discover<TClient>()
        {
            var serviceDiscovery = _serviceProvider.GetService<IServiceDiscovery>();
            var serviceRegistration = serviceDiscovery.GetService<TClient>();
            if (serviceRegistration == null)
                throw new Exception($"Service {typeof(TClient).FullName} is not registered or not healthy");

            var clientProxy = DispatchProxy.Create<TClient, ClientProxyBase>();
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = serviceRegistration.ServiceUri;

            if (string.IsNullOrEmpty(serviceRegistration.ServiceProtocols))
                throw new Exception($"Service {typeof(TClient).FullName} doesn't have protocols infomation");

            var protocols = serviceRegistration.ServiceProtocols.Split(new char[] { ',' });
            var rpcCallFactory = _rpcCallsMapping[protocols[0]];

            (clientProxy as ClientProxyBase).HttpClient = httpClient;
            (clientProxy as ClientProxyBase).ServiceType = typeof(TClient);
            (clientProxy as ClientProxyBase).RpcCalls = rpcCallFactory(httpClient);

            return clientProxy;
        }

        private static Dictionary<string, Func<HttpClient, IFakeRpcCalls>> _rpcCallsMapping =
            new Dictionary<string, Func<HttpClient, IFakeRpcCalls>>()
            {
                { FakeRpcMediaTypes.Default, DefaultFakeRpcCalls.Factory },
                { FakeRpcMediaTypes.MessagePack, MessagePackRpcCalls.Factory},
                { FakeRpcMediaTypes.Protobuf,  ProtobufRpcCalls.Factory }
            };
    }
}
