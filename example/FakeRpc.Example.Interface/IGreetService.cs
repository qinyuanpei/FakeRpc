using FakeRpc.Core;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Example.Interface
{
    /// <summary>
    /// IGreetService
    /// </summary>
    [FakeRpc]
    public interface IGreetService
    {
        /// <summary>
        /// SayHello
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HelloReply> SayHello(HelloRequest request);

        /// <summary>
        /// SayWho
        /// </summary>
        /// <returns></returns>
        Task<HelloReply> SayWho();
    }

    /// <summary>
    /// HelloReply
    /// </summary>
    [Serializable]
    [ProtoContract]
    [MessagePackObject]
    public class HelloReply
    {
        /// <summary>
        /// Message
        /// </summary>
        [Key(0)]
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    /// <summary>
    /// HelloRequest
    /// </summary>
    [Serializable]
    [ProtoContract]
    [MessagePackObject]
    public class HelloRequest
    {
        /// <summary>
        /// Name
        /// </summary>
        [Key(0)]
        [ProtoMember(1)]
        public string Name { get; set; }
    }
}
