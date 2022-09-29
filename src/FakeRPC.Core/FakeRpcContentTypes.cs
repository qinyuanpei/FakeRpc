using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcContentTypes
    {
        public const string Default = "application/json";
        public const string MessagePack = "application/x-msgpack";
        public const string Protobuf = "application/x-protobuf";
        public const string FlatBuffer = "application/x-flatbuffer";

        public static IEnumerable<string> SupportedContentTypes()
        {
            yield return Default;
            yield return Protobuf;
            yield return MessagePack;
            yield return FlatBuffer;
        }
    }
}
