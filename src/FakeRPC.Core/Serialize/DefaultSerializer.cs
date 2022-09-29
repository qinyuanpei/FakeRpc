using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Serialize
{
    public class DefaultSerializer : IMessageSerializer
    {
        public byte[] Serialize<TMessage>(TMessage message) where TMessage : class
        {
            var jsonify = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(jsonify);
        }

        public Task<byte[]> SerializeAsync<TMessage>(TMessage message) where TMessage : class
        {
            var jsonify = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonify);
            return Task.FromResult(bytes);
        }

        public TMessage Deserialize<TMessage>(byte[] bytes) where TMessage : class
        {
            var jsonify = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<TMessage>(jsonify);
        }

        public Task<TMessage> DeserializeAsync<TMessage>(byte[] bytes) where TMessage : class
        {
            var jsonify = Encoding.UTF8.GetString(bytes);
            var messgae = JsonConvert.DeserializeObject<TMessage>(jsonify);
            return Task.FromResult(messgae);
        }
    }
}
