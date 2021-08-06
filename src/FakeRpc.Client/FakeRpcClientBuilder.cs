using Consul;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.LoadBalance;
using FakeRpc.Core.Mics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace FakeRpc.Client
{
    public class FakeRpcClientBuilder
    {
        private readonly IServiceCollection _services;

        public IServiceCollection Services => _services;

        public FakeRpcClientBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public FakeRpcClientBuilder AddRpcClient<TClient>(Action<HttpClient> configureClient)
        {
            _services.AddHttpClient(typeof(TClient).GetServiceName(), configureClient);
            _services.AddSingleton<FakeRpcClientFactory>();
            return this;
        }

        public FakeRpcClientBuilder AddRpcCallsFactory(Func<HttpClient, IFakeRpcCalls> factory = null)
        {
            if (factory == null)
                factory = httpClient => new DefaultFakeRpcCalls(httpClient);

            _services.AddSingleton(factory);
            return this;
        }

        public FakeRpcClientBuilder EnableServiceDiscovery<TServiceDiscovery>(Func<IServiceProvider, TServiceDiscovery> serviceDiscoveryFactory = null) where TServiceDiscovery : class, IServiceDiscovery
        {
            if (serviceDiscoveryFactory != null)
                _services.AddSingleton<TServiceDiscovery>(serviceDiscoveryFactory);
            else
                _services.AddSingleton<IServiceDiscovery, TServiceDiscovery>();

            return this;
        }

        public FakeRpcClientBuilder EnableLoadBalance<TStrategy>() where TStrategy: class, ILoadBalanceStrategy
        {
            _services.AddTransient<ILoadBalanceStrategy, TStrategy>();
            return this;
        }


        public void Build()
        {
            var serviceProvider = _services.BuildServiceProvider(); 
        }
    }
}
