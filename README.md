# FakeRPC

一个基于 `ASP.NET Core` 的轻量级 `RPC` 框架，因其通信建立在 `HTTP` 协议而非 `TCP/IP` 协议上，故不能称之为真正的 `RPC` 框架。由此得名：`FakeRPC`。

话虽如此，`FakeRPC` 实现了主流 `RPC` 框架中常见的功能，例如：灵活、多样化的序列化/反序列化方案、客户端动态代理、服务注册、服务发现、负载均衡、接口文档等等。

* [x] 序列化/反序列化：支持`JSON`、`Protobuf`、`MesssagePack`
* [x] 客户端动态代理：基于`DispatchProxy` 和 `HttpCleintFactory`
* [x] 服务发现/注册：支持`Redis`、`Consul`、`Nacos`
* [x] 接口文档：基于`Swagger`的接口文档
* [ ] 负载均衡器：正在开发中...


# 如何使用

* 编写服务

* 配置服务端

* 配置客户端

# 更多


