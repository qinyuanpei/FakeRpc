using Consul;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.LoadBalance;
using FakeRpc.Core.LoadBalance.Strategy;
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
            _services.AddHttpClient(typeof(TClient).GetClientName(), configureClient);
            _services.AddSingleton<FakeRpcClientFactory>();
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

        public FakeRpcClientBuilder WithLoadBalanceStrategy(LoadBalanceStrategy loadBalanceStrategy)
        {
            switch (loadBalanceStrategy)
            {
                case LoadBalanceStrategy.Random:
                    _services.AddTransient<ILoadBalanceStrategy, RandomStrategy>();
                    break;
                case LoadBalanceStrategy.RandomWithWeight:
                    _services.AddTransient<ILoadBalanceStrategy, RandomWithWeightStrategy>();
                    break;
                case LoadBalanceStrategy.RoundRobin:
                    _services.AddSingleton<ILoadBalanceStrategy, RoundRobinStrategy>();
                    break;
                case LoadBalanceStrategy.RoundRobinWithWeight:
                    _services.AddSingleton<ILoadBalanceStrategy, RoundRobinWithWeightStrategry>();
                    break;
                case LoadBalanceStrategy.IpHash:
                    _services.AddTransient<ILoadBalanceStrategy, IpHashStrategy>();
                    break;

            }
            
            return this;
        }


        public void Build()
        {
            var serviceProvider = _services.BuildServiceProvider(); 
        }
    }
}
