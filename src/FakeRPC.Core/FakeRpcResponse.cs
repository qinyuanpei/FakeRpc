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
    public class FakeRpcResponse
    {
        [Key(0)]
        [ProtoMember(1)]
        public string Id { get; set; }

        [Key(1)]
        [ProtoMember(2)]
        public string Result { get;  set; }

        [Key(2)]
        [ProtoMember(3)]
        public string Error { get; set; }

        public void SetResult(dynamic obj) => Result = JsonConvert.SerializeObject(obj);

        public byte[] ToByteArray()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
