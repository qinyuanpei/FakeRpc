using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Tcp
{
    public class FakeRpcMessageParser : IFakeRpcMessageParser
    {
        public bool TryParseMessage<TMessage>(ReadOnlySequence<byte> buffer, out TMessage message)
        {
            var bytes = buffer.ToArray();
            var json = Encoding.UTF8.GetString(bytes);
            message = JsonConvert.DeserializeObject<TMessage>(json);
            return true;
        }
    }
}
