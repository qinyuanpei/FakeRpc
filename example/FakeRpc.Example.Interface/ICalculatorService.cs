using FakeRpc.Core;
using FlatSharp.Attributes;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Example.Interface
{
    [FakeRpc(ServiceGroup = "FakeRpc.Example.Interface.Calculator", ServiceName = "Calculator")]
    public interface ICalculatorService
    {
        Task<CalculatorReply> Calculate(CalculatorRequest request);
        Task<CalculatorReply> Random();
    }

    [Serializable]
    [ProtoContract]
    [MessagePackObject]
    [FlatBufferTable]
    public class CalculatorReply
    {
        [Key(0)]
        [ProtoMember(1)]
        [FlatBufferItem(0)]
        public string Expression { get; set; }
        [Key(1)]
        [ProtoMember(2)]
        [FlatBufferItem(1)]
        public decimal Result { get; set; }
    }

    [Serializable]
    [ProtoContract]
    [MessagePackObject]
    [FlatBufferTable]
    public class CalculatorRequest
    {
        [Key(0)]
        [ProtoMember(1)]
        [FlatBufferItem(0)]
        public string Op { get; set; }
        [Key(1)]
        [ProtoMember(2)]
        [FlatBufferItem(1)]
        public decimal Num1 { get; set; }
        [Key(2)]
        [ProtoMember(3)]
        [FlatBufferItem(2)]
        public decimal Num2 { get; set; }
    }
}
