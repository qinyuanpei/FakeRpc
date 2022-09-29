using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatSharp;

namespace FakeRpc.Core.Serialize
{
    public class FlatSharpSerializer : IMessageSerializer
    {
        public byte[] Serialize<TMessage>(TMessage message) where TMessage : class
        {
            var bufferMaxSize = FlatBufferSerializer.Default.GetMaxSize(message);
            var bufferSpan = new byte[bufferMaxSize].AsSpan();
            int bytesWritten = FlatBufferSerializer.Default.Serialize(message, bufferSpan);
            return bufferSpan.Slice(0, bytesWritten).ToArray();
        }

        public Task<byte[]> SerializeAsync<TMessage>(TMessage message) where TMessage : class
        {
            var bufferMaxSize = FlatBufferSerializer.Default.GetMaxSize(message);
            var bufferSpan = new byte[bufferMaxSize].AsSpan();
            int bytesWritten = FlatBufferSerializer.Default.Serialize(message, bufferSpan);
            var bytesArray = bufferSpan.Slice(0, bytesWritten).ToArray();
            return Task.FromResult(bytesArray);
        }

        public TMessage Deserialize<TMessage>(byte[] bytes) where TMessage : class
        {
            return FlatBufferSerializer.Default.Parse<TMessage>(bytes);
        }

        public Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes) where TMessage : class
        {
            return Task.FromResult(FlatBufferSerializer.Default.Parse<TMessage>(bytes));
        }
    }
}
