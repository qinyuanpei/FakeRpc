using FakeRpc.Core.Mics;
using MessagePack;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    [ProtoContract]
    [MessagePackObject]
    public class FakeRpcRequest
    {
        [Key(0)]
        [ProtoMember(1)]
        public string Id { get; set; }

        [Key(1)]
        [ProtoMember(2)]
        public string ServiceName { get; set; }

        [Key(2)]
        [ProtoMember(3)]
        public string ServiceGroup { get; set; }

        [Key(3)]
        [ProtoMember(4)]
        public string MethodName { get; set; }

        [Key(4)]
        [ProtoMember(5)]
        public string MethodParams { get; set; }

        public static FakeRpcRequest Parse(string value) => 
            JsonConvert.DeserializeObject<FakeRpcRequest>(value);

        public static FakeRpcRequest Create(Type type) => 
            new FakeRpcRequest() { Id = Guid.NewGuid().ToString("N"), ServiceGroup = type.GetServiceGroup(), ServiceName = type.GetServiceName() };

        public static FakeRpcRequest Create<T>() =>
            new FakeRpcRequest() { Id = Guid.NewGuid().ToString("N"), ServiceGroup = typeof(T).GetServiceGroup(), ServiceName = typeof(T).GetServiceName() };
    }

    public class FakeRpcRequest<TParameter>
    {
        public TParameter MethodParam { get; set; }
    }
}
