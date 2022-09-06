using FakeRpc.Core.Mics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcRequest
    {
        public string Id { get; set; }

        public string ServiceName { get; set; }

        public string ServiceGroup { get; set; }

        public string MethodName { get; set; }

        public KeyValuePair<string, object>[] MethodParams { get; set; }

        public static FakeRpcRequest Parse(string value) => 
            JsonConvert.DeserializeObject<FakeRpcRequest>(value);

        public static FakeRpcRequest Create(Type type) => 
            new FakeRpcRequest() { Id = Guid.NewGuid().ToString("N"), ServiceGroup = type.GetServiceGroup(), ServiceName = type.GetServiceName() };

        public static FakeRpcRequest Create<T>() =>
            new FakeRpcRequest() { Id = Guid.NewGuid().ToString("N"), ServiceGroup = typeof(T).GetServiceGroup(), ServiceName = typeof(T).GetServiceName() };
    }
}
