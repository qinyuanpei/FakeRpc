using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Serialize
{
    public interface IMessageSerializer
    {
        // Serialize
        byte[] Serialize<TMessage>(TMessage message) where TMessage : class;
        Task<byte[]> SerializeAsync<TMessage>(TMessage message) where TMessage : class;

        // Deserialize
        TMessage Deserialize<TMessage>(byte[] bytes) where TMessage : class;
        Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes) where TMessage : class;
    }
}
