using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Serialize
{
     public class ProtobufSerializer : IMessageSerializer
     {
        public byte[] Serialize<TMessage>(TMessage message) where TMessage : class
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                return stream.ToArray();
            }
        }

        public Task<byte[]> SerializeAsync<TMessage>(TMessage message) where TMessage : class
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                return Task.FromResult(stream.ToArray());
            }
        }

        public TMessage Deserialize<TMessage>(byte[] bytes) where TMessage : class
        {
            using (var stream= new MemoryStream(bytes))
            {
                return Serializer.Deserialize<TMessage>(stream);
            }
        }

        public Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes) where TMessage : class
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Task.FromResult(Serializer.Deserialize<TMessage>(stream));
            }
        }
     }
}
