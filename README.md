# FakeRPC

![GitHub](https://img.shields.io/github/license/qinyuanpei/FakeRpc) ![GitHub Workflow Status](https://img.shields.io/github/workflow/status/qinyuanpei/FakeRpc/Release) 


一个基于 `ASP.NET Core` 的轻量级 `RPC` 框架，因其通信建立在 `HTTP` 协议而非 `TCP/IP` 协议上，故不能称之为真正的 `RPC` 框架。由此得名：`FakeRPC`。

![ FakeRpc 原理图](https://raw.fastgit.org/qinyuanpei/FakeRpc/master/src/Screenshots/FakeRpc.png)

# 安装方法

请按需选择合适的库进行安装:

```
dotnet add package FakeRpc.Core
dotnet add package FakeRpc.Server
dotnet add package FakeRpc.Client
dotnet add package FakeRpc.ServiceRegistry.Consul
dotnet add package FakeRpc.ServiceRegistry.Nacos
dotnet add package FakeRpc.ServiceDiscovery.Consul
dotnet add package FakeRpc.ServiceDiscovery.Nacos
```

# 主要特性

* [x] 序列化/反序列化：支持 `JSON` 、 `Protobuf` 、 `MesssagePack` 
* [x] 客户端动态代理：基于 `DispatchProxy` 和 `HttpCleintFactory`
* [x] 服务发现/注册：支持 `Redis` 、 `Consul` 、`Nacos`
* [x] 接口文档：基于 `Swagger` 的接口文档
* [x] 负载均衡器：支持 `随机` 、 `轮询` 、 `哈希一致性` 等


# 如何使用

* 编写服务

定义一个接口，并附加`[FakeRpc]`特性。按照约定，接口返回值必须定义为`Task<T>`，参数最多一个，参考 `gRPC`, 该参数不能是普通的基础类型，如 `int`、`string` 等等：

```csharp
[FakeRpc]
public interface IGreetService
{
    Task<HelloReply> SayHello(HelloRequest request);
    Task<HelloReply> SayWho();
}
```

定义实体，并附加序列化相关的特性，`MessagePack`可以忽略`[Key]`标签，`Protobuf`必须添加`[ProtoMember]`标签：

```csharp
[Serializable]
[ProtoContract]
[MessagePackObject]
public class HelloReply
{
    [Key(0)]
    [ProtoMember(1)]
    public string Message { get; set; }
}

[Serializable]
[ProtoContract]
[MessagePackObject]
public class HelloRequest
{
    [Key(0)]
    [ProtoMember(1)]
    public string Name { get; set; }
}
```

实现`IGreetServer`接口如下：

```csharp
public class GreetService : IGreetService
{

    public Task<HelloReply> SayHello(HelloRequest request)
    {
        return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}" });
    }

    public Task<HelloReply> SayWho()
    {
        return Task.FromResult(new HelloReply { Message = $"I'm 长安书小妆" });
    }
}
```

更多细节，请参考：[FakeRpc.Example.Interface](https://hub.fastgit.org/qinyuanpei/FakeRpc/tree/master/example/FakeRpc.Example.Interface/)。

* 配置服务端

新建一个`ASP.NET Core`项目，添加对`FakeRpc.Core`和`FakeRpc.Server`两个程序集的引用：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ...
    var builder = new FakeRpcServerBuilder(services);
    builder
        .AddFakeRpc()
        .UseMessagePack()
        .UseUseProtobuf()
        .EnableSwagger()
        .AddExternalAssembly(typeof(GreetService).Assembly)
        .EnableNacosServiceRegistry(options => options.ServerAddress = new List<string> { "http://localhost:8848" });
             
    builder.Build();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
{

    // ...
    app.UseFakeRpc(applicationLifetime);
}
```

以上为服务端最小化配置，如果需要开启接口文档，则可以追加以下语句：`EnableSwagger()`。

如果服务在当前程序集下，则可以忽略`AddExternalAssembly()`方法；如果服务不在当前程序集下，需要显式地指定服务所在的程序集，框架会自动完成服务的扫描工作。

如果需要服务注册功能，则可以引用内置的扩展包，例如：`FakeRpc.ServiceRegistry.Nacos`，同时加入下列语句：

```csharp
builder.EnableNacosServiceRegistry(options => 
{
    options.ServerAddress = new List<string> { "http://localhost:8848" };
});
```

更多细节，请参考：[ServerExample](https://hub.fastgit.org/qinyuanpei/FakeRpc/tree/master/src/Example/FakeRpc.Web)。

* 配置客户端

客户端通过接口来生成动态代理，像调用本地方法一样调用一个远程服务：

```csharp
var services = new ServiceCollection();

var builder = new FakeRpcClientBuilder(services);

builder.AddRpcClient<IGreetService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
    client.DefaultRequestVersion = new Version(1, 0);
});
```

以上，为客户端部分最小化配置，和 `gRPC` 类似，通过 `FakeRpcClientFactory` 接口来创建一个客户端：

```csharp
var _clientFactory = serviceProvider.GetService<FakeRpcClientFactory>();
var greetProxy = _clientFactory.Create<IGreetService>();
var reply = await greetProxy.SayHello(new HelloRequest() { Name = "张三" });
reply = await greetProxy.SayWho();
```

客户端可以自由决定使用什么样的序列化/反序列化协议：

```csharp

// 全局注入
builder.AddRpcCallsFactory(MessagePackRpcCalls.Factory);

// 局部指定
var greetProxy = _clientFactory.Create<IGreetService>(MessagePackRpcCalls.Factory)
```
客户端可以使用服务发现，以 `Consul` 为例：

```csharp
builder.EnableConsulServiceDiscovery(options =>
{
    options.BaseUrl = "http://localhost:8500";
    options.UseHttps = true;
});
```

更多细节，请参考：[FakeRpc.Eaxmple.Client](https://hub.fastgit.org/qinyuanpei/FakeRpc/tree/master/src/example/FakeRpc.Eaxmple.Client)。

# 更多

Todo

