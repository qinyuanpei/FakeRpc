using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Tcp
{
    public interface IFakeRpcMessageParser
    {
        bool TryParseMessage<TMessage>(ReadOnlySequence<byte> buffer, out TMessage message);


    }
}
