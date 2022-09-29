using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FakeRpc.Core;
using FakeRpc.Client;
using FakeRpc.ServiceRegistry.Nacos;
using FakeRpc.Core.LoadBalance;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FakeRpc.Example.Interface;
using FakeRpc.ServiceDiscovery.Consul;
using FakeRpc.Core.Discovery;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using FakeRpc.Core.Mics;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using FakeRpc.Core.Invokers.WebSockets;
using FakeRpc.Core.Invokers.Tcp;

namespace ClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //BenchmarkRunner.Run<TestContext>();
            var serviceProvider = new TestContext().InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("http://localhost:8099"), FakeRpcTransportProtocols.Tcp, FakeRpcContentTypes.MessagePack);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("http://localhost:8099"), FakeRpcTransportProtocols.Tcp, FakeRpcContentTypes.Default);
            var result = calculatorProxy.Random();
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    [RPlotExporter]
    public class TestContext
    {
        public IServiceProvider InitIoc()
        {
            var services = new ServiceCollection();

            services.AddLogging(option => option.AddConsole());
            services.AddTransient<IClientWebSocketCallInvoker, ClientWebSocketCallInvoker>();
            services.AddTransient<IClientStreamingCallInvoker, ClientStreamingCallInvoker>();

            var builder = new FakeRpcClientBuilder(services);

            builder.AddRpcClient<IGreetService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestVersion = new Version(1, 0);
            });

            builder.AddRpcClient<ICalculatorService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5000");
                client.DefaultRequestVersion = new Version(1, 0);
            });

            builder.WithLoadBalanceStrategy(LoadBalanceStrategy.Random);
            builder.EnableNacosServiceDiscovery(options =>
            {
                options.ServerAddress = new List<string> { "http://localhost:8848" };
            });

            return services.BuildServiceProvider();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with MessagePack & Http", OperationsPerInvoke = 1)]
        public async Task RunHttpMessagePack()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.MessagePack);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.MessagePack);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with Protobuff & Http", OperationsPerInvoke = 1)]
        public async Task RunHttpProtobuf()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.Protobuf);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.Protobuf);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with JSON & Http", OperationsPerInvoke = 1)]
        public async Task RunHttpJson()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.Default);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("http://localhost:5000"), FakeRpcTransportProtocols.Http, FakeRpcContentTypes.Default);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with JSON & WebSocket", OperationsPerInvoke = 1)]
        public async Task RunWebSocketWithJson()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Default);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Default);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with MessagePack & WebSocket", OperationsPerInvoke = 1)]
        public async Task RunWebSocketWithMessagePack()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.MessagePack);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.MessagePack);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with Protobuf & WebSocket", OperationsPerInvoke = 1)]
        public async Task RunWebSocketWithProtobuf()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Protobuf);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Protobuf);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with FlatBuffer & WebSocket", OperationsPerInvoke = 1)]
        public async Task RunWebSocketWithFlatBuffer()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Protobuf);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(new Uri("ws://localhost:5000"), FakeRpcTransportProtocols.WebSocket, FakeRpcContentTypes.Protobuf);
            var result = calculatorProxy.Random();
        }
    }
}
