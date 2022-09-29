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
                case FakeRpcContentTypes.Default:
                    return new DefaultSerializer();
                case FakeRpcContentTypes.Protobuf:
                    return new ProtobufSerializer();
                case FakeRpcContentTypes.MessagePack:
                    return new MessagePackSerializer();
                case FakeRpcContentTypes.FlatBuffer:
                    return new FlatSharpSerializer();
                default:
                    return new DefaultSerializer();
            }
        }
    }
}
