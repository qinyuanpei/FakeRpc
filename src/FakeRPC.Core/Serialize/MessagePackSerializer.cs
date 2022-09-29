using MessagePack;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Serialize
{
    public class MessagePackSerializer : IMessageSerializer
    {
        private readonly MessagePackSerializerOptions _options;
        public MessagePackSerializer()
        {
            _options = ContractlessStandardResolver.Options.WithCompression(MessagePackCompression.Lz4Block);
        }

        public byte[] Serialize<TMessage>(TMessage message) where TMessage : class
        {
            using (var stream = new MemoryStream())
            {
                MessagePack.MessagePackSerializer.Serialize(stream, message, _options);
                return stream.ToArray();
            }
        }

        public async Task<byte[]> SerializeAsync<TMessage>(TMessage message) where TMessage : class
        {
            using (var stream = new MemoryStream())
            {
                await MessagePack.MessagePackSerializer.SerializeAsync(stream, message, _options);
                return stream.ToArray();
            }
        }

        public TMessage Deserialize<TMessage>(byte[] bytes) where TMessage : class
        {
            using (var readStream = new MemoryStream(bytes))
            {
                return MessagePack.MessagePackSerializer.Deserialize<TMessage>(readStream, _options);
            }
        }

        public Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes) where TMessage : class
        {
            using (var readStream = new MemoryStream(bytes))
            {
                var message = MessagePack.MessagePackSerializer.Deserialize<TMessage>(readStream, _options);
                return Task.FromResult(message);
            }
        }
    }
}
