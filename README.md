# FakeRPC

一个基于 `ASP.NET Core` 的轻量级 `RPC` 框架，因其通信建立在 `HTTP` 协议而非 `TCP/IP` 协议上，故不能称之为真正的 `RPC` 框架。由此得名：`FakeRPC`。

![ FakeRpc 原理图](https://raw.fastgit.org/qinyuanpei/FakeRpc/master/src/Screenshots/FakeRpc.png)

话虽如此，`FakeRPC` 实现了主流 `RPC` 框架中常见的功能，例如：灵活、多样化的序列化/反序列化方案、客户端动态代理、服务注册、服务发现、负载均衡、接口文档等等。

* [x] 序列化/反序列化：支持`JSON`、`Protobuf`、`MesssagePack`
* [x] 客户端动态代理：基于`DispatchProxy` 和 `HttpCleintFactory`
* [x] 服务发现/注册：支持`Redis`、`Consul`、`Nacos`
* [x] 接口文档：基于`Swagger`的接口文档
* [ ] 负载均衡器：正在开发中...


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

定义实体，并附加序列化相关的特性：

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

更多细节，请参考：[ServerExample](https://hub.fastgit.org/qinyuanpei/FakeRpc/tree/master/src/Example/FakeRpc.Web)。

* 配置服务端

新建一个`ASP.NET Core`项目，添加对`FakeRpc.Core`和`FakeRpc.Server`两个程序集的引用：

```csharp
services.AddControllers();

var builder = new FakeRpcServerBuilder(services);
builder
    .AddFakeRpc()
    .UseMessagePack()
    .UseUseProtobuf()
    .AddExternalAssembly(typeof(GreetService).Assembly)
builder.Build();
```

以上为服务端最小化配置，如果需要开启接口文档，则可以追加以下语句：`EnableSwagger()`。

如果服务在当前程序集下，则可以忽略`AddExternalAssembly()`方法；如果服务不在当前程序集下，需要显式地指定服务所在的程序集，框架会自动完成服务的扫描工作。

如果需要服务注册功能，则可以引用内置的扩展包，例如：`FakeRpc.ServiceRegistry.Nacos`，同时加入下列语句：

```csharp
builder.EnableNacosServiceRegistry(options => 
{
    options.ServerAddress = new List<string> { "http://192.168.50.162:8848" };
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

更多细节，请参考：[ClientExample](https://hub.fastgit.org/qinyuanpei/FakeRpc/tree/master/src/Example/FakeRpc.Client)。

# 更多


