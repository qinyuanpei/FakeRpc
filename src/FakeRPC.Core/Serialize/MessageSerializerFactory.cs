using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Serialize
{
    public class MessageSerializerFactory
    {
        public static IMessageSerializer Create(string contentType)
        {
            switch (contentType)
            {
                case FakeRpcMediaTypes.Default:
                    return new DefaultSerializer();
                case FakeRpcMediaTypes.Protobuf:
                    return new ProtobufSerializer();
                case FakeRpcMediaTypes.MessagePack:
                    return new MessagePackSerializer();
                default:
                    return new DefaultSerializer();
            }
        }
    }
}
