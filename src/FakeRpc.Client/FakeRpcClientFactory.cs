using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.Invokers.Http;
using FakeRpc.Core.Invokers.WebSockets;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        private readonly ILoggerFactory _loggerFactory;

        public FakeRpcClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
        }

        public TClient Create<TClient>(Uri baseUri, FakeRpcTransportProtocols transportProtocols = FakeRpcTransportProtocols.Http, string contentType = FakeRpcContentTypes.Default)
        {
            switch (transportProtocols)
            {
                case FakeRpcTransportProtocols.Http:
                    return CreateHttpClient<TClient>(baseUri, contentType);
                case FakeRpcTransportProtocols.WebSocket:
                    return CreateWebSocketClient<TClient>(baseUri, contentType);
                case FakeRpcTransportProtocols.Tcp:
                    break;
            }

            throw new ArgumentException($"The specified transport protocol {Enum.GetName(typeof(FakeRpcTransportProtocols), transportProtocols)} does not support.");
        }

        public TClient Discover<TClient>(FakeRpcTransportProtocols transportProtocols = FakeRpcTransportProtocols.Http, string contentType = FakeRpcContentTypes.Default)
        {
            var serviceDiscovery = _serviceProvider.GetService<IServiceDiscovery>();
            var serviceRegistration = serviceDiscovery.GetService<TClient>();
            if (serviceRegistration == null)
                throw new Exception($"Service {typeof(TClient).FullName} is not registered or not healthy");

            return Create<TClient>(serviceRegistration.ServiceUri, transportProtocols, contentType);
        }

        private TClient CreateHttpClient<TClient>(Uri baseUri, string contentType = FakeRpcContentTypes.Default)
        {
            var httpClientFactory = _serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = baseUri;

            var serializer = MessageSerializerFactory.Create(contentType);

            var clientProxy = DispatchProxy.Create<TClient, HttpClientProxy<TClient>>();
            (clientProxy as HttpClientProxy<TClient>).HttpClient = httpClient;
            (clientProxy as HttpClientProxy<TClient>).CallInvoker = new HttpCallInvoker(httpClient, serializer);

            return clientProxy;
        }

        private TClient CreateHttpClient<TClient>(string baseUrl, string contentType = FakeRpcContentTypes.Default)
        {
            var baseUri = new Uri(baseUrl);
            return CreateHttpClient<TClient>(baseUri, contentType);
        }

        private TClient CreateWebSocketClient<TClient>(Uri baseUrl, string contentType = FakeRpcContentTypes.Default)
        {
            var formatedUrl = baseUrl.AbsoluteUri.Contains("?") ? new Uri($"{baseUrl.AbsoluteUri}&{Constants.FAKE_RPC_HEADER_CONTENT_TYPE}={contentType}") :
                new Uri($"{baseUrl.AbsoluteUri}?{Constants.FAKE_RPC_HEADER_CONTENT_TYPE}={contentType}");

            var serializationHandler = MessageSerializerFactory.Create(contentType);

            var webSocket = new ClientWebSocket();
            var clientProxy = DispatchProxy.Create<TClient, WebSocketClientProxy<TClient>>();
            (clientProxy as WebSocketClientProxy<TClient>).Uri = formatedUrl;
            (clientProxy as WebSocketClientProxy<TClient>).WebSocket = webSocket;
            (clientProxy as WebSocketClientProxy<TClient>).CallInvoker = new ClientWebSocketCallInvoker(_serviceProvider, webSocket, serializationHandler);
            (clientProxy as WebSocketClientProxy<TClient>).Logger = _serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<WebSocketClientProxy<TClient>>();

            // 提前连接，以减少接口调用时长
            ((clientProxy as WebSocketClientProxy<TClient>).WebSocket as ClientWebSocket).ConnectAsync(formatedUrl, CancellationToken.None).GetAwaiter().GetResult();

            return clientProxy;
        }

        private TClient CreateWebSocketClient<TClient>(string baseUrl, string contentType = FakeRpcContentTypes.Default)
        {
            return CreateWebSocketClient<TClient>(new Uri(baseUrl), contentType);
        }
    }
}
