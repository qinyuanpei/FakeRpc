using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Serialize
{
    public interface IMessageSerializer
    {
        // Serialize
        byte[] Serialize<TMessage>(TMessage message);
        Task<byte[]> SerializeAsync<TMessage>(TMessage message);

        // Deserialize
        TMessage Deserialize<TMessage>(byte[] bytes);
        Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes);
    }
}
