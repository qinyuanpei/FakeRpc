using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FakeRpc.Core;
using FakeRpc.Client;
using FakeRpc.Client.Netty;
using FakeRpc.ServiceRegistry.Nacos;
using FakeRpc.Core.LoadBalance;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FakeRpc.Example.Interface;
using FakeRpc.ServiceDiscovery.Consul;
using FakeRpc.Core.Discovery;
using System.Collections.Generic;

namespace ClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //BenchmarkRunner.Run<TestContext>();
            //Console.ReadKey();

            var services = new ServiceCollection();
            var builder = new FakeRpcClientBuilder(services);
            builder.AddNetty();
            builder.UseNetty().Wait();
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net50)]
    [RPlotExporter]
    public class TestContext
    {
        public IServiceProvider InitIoc()
        {
            var services = new ServiceCollection();

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

            builder.AddRpcCallsFactory(MessagePackRpcCalls.Factory);
            builder.WithLoadBalanceStrategy(LoadBalanceStrategy.Random);
            builder.EnableNacosServiceDiscovery(options =>
            {
                options.ServerAddress = new List<string> { "http://localhost:8848" };
            });

            return services.BuildServiceProvider();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with MessagePack", OperationsPerInvoke = 1)]
        public async Task RunMessagePack()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(MessagePackRpcCalls.Factory);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(MessagePackRpcCalls.Factory);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with Protobuff", OperationsPerInvoke = 1)]
        public async Task RunProtobuf()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(ProtobufRpcCalls.Factory);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(ProtobufRpcCalls.Factory);
            var result = calculatorProxy.Random();
        }

        [Benchmark(Baseline = false, Description = "Test FakeRpc with JSON", OperationsPerInvoke = 1)]
        public async Task RunJson()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.Create<IGreetService>(DefaultFakeRpcCalls.Factory);
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.Create<ICalculatorService>(DefaultFakeRpcCalls.Factory);
            var result = calculatorProxy.Random();
        }
    }
}
