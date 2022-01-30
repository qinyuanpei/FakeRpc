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
using System.Net.Sockets;
using FakeRpc.Core.Mics;
using Newtonsoft.Json;
using System.Text;

namespace ClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //BenchmarkRunner.Run<TestContext>();

            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("192.168.50.162", 5010);

            var stream = tcpClient.GetStream();

            var request = new FakeRpc.Core.Tcp.FakeRpcRequest();
            request.Id = Guid.NewGuid().ToString("N");
            request.ServiceName = typeof(IGreetService).GetServiceName();
            request.ServiceGroup = typeof(IGreetService).GetServiceGroup();
            request.MethodName = "SayHello";
            request.MethodParams =  new KeyValuePair<string, object>[] {
                new KeyValuePair<string, object>("request", new HelloRequest() { Name = "飞鸿踏雪" })
            };

            var json = JsonConvert.SerializeObject(request);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(json));
            Console.WriteLine("  Request=>\r\n{0}\r\n", json);

            var bytes = new byte[256];
            await stream.ReadAsync(bytes);
            json = Encoding.UTF8.GetString(bytes);
            Console.WriteLine("  Response=>\r\n{0}", json);

            Console.ReadKey();
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
