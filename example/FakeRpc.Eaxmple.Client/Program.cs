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
using FakeRpc.Client.WebSockets;
using FakeRpc.Core.WebSockets;

namespace ClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new TestContext().InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.CreateSocketClient<IGreetService>("ws://localhost:5000");
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.CreateSocketClient<ICalculatorService>("ws://localhost:5000");
            var result = calculatorProxy.Random();

            //BenchmarkRunner.Run<TestContext>();
            //var socket = new ClientWebSocket();
            //await socket.ConnectAsync(new Uri("ws://localhost:5000"), CancellationToken.None);

            //var serviceProvider = new TestContext().InitIoc();
            //var clientRpcBinder = serviceProvider.GetService<ISocketRpcBinder>();
            //clientRpcBinder.OnSend += req => Console.WriteLine("Sent: {0}", JsonConvert.SerializeObject(req));
            //clientRpcBinder.OnReceive += res => Console.WriteLine("Received: {0}", JsonConvert.SerializeObject(res));

            //var i = 0;
            //while (i < 10)
            //{
            //    // SayHello
            //    var request = new FakeRpcRequest()
            //    {
            //        Id = Guid.NewGuid().ToString("N"),
            //        ServiceGroup = typeof(IGreetService).GetServiceGroup(),
            //        ServiceName = typeof(IGreetService).GetServiceName(),
            //        MethodName = "SayHello",
            //        MethodParams = new KeyValuePair<string, object>[]
            //        {
            //            new KeyValuePair<string, object>("request", new HelloRequest(){ Name = "飞鸿踏雪" })
            //        }
            //    };
            //    await Task.Delay(1000);
            //    await clientRpcBinder.Invoke(request, socket);

            //    // Calculate
            //    var random = new Random();
            //    request = new FakeRpcRequest()
            //    {
            //        Id = Guid.NewGuid().ToString("N"),
            //        ServiceGroup = typeof(ICalculatorService).GetServiceGroup(),
            //        ServiceName = typeof(ICalculatorService).GetServiceName(),
            //        MethodName = "Calculate",
            //        MethodParams = new KeyValuePair<string, object>[]
            //        {
            //            new KeyValuePair<string, object>("request", new CalculatorRequest{ Op = "+", Num1 = random.Next(0, 100), Num2 = random.Next(0, 100) })
            //        }
            //    };
            //    await Task.Delay(1000);
            //    await clientRpcBinder.Invoke(request, socket);
            //    i++;
            //}

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

            services.AddLogging();
            services.AddTransient<ISocketRpcBinder, ClientRpcBinder>();

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

        [Benchmark(Baseline = false, Description = "Test FakeRpc with JSON & WebSocket", OperationsPerInvoke = 1)]
        public async Task RunWebSocket()
        {
            var serviceProvider = InitIoc();
            var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
            var greetProxy = _clientFactory.CreateSocketClient<IGreetService>("ws://localhost:5000");
            var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
            reply = await greetProxy.SayWho();
            var calculatorProxy = _clientFactory.CreateSocketClient<ICalculatorService>("ws://localhost:5000");
            var result = calculatorProxy.Random();
        }
    }
}
