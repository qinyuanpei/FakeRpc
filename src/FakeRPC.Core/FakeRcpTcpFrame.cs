using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core
{
    // This is the binary protocol of TcpFrame for FakeRPC
    // TotalLength：Header Length + Header + Body, 4 bytes
    // HeaderLength: Length of Header, 4 bytes
    // Header: Key-Value Pairs
    // Body: Payload 
    // |------------------------------------------------------| //
    // |- Total Length -|- Header Length -|- Header -|- Body -| //
    // |------------------------------------------------------| //
    // 

    public class FakeRpcTcpFrame<TBody>
    {
        public int TotalLength { get; private set; }

        public int HeaderLength { get; private set; }

        public TBody Body { get; set; }

        public Dictionary<string, string> Header { get; set; }

        public FakeRpcTcpFrame()
        {

        }

        public FakeRpcTcpFrame(TBody body, Dictionary<string, string> header, int totalLength, int headerLength)
        {
            Body = body;
            Header = header;
            TotalLength = totalLength;
            HeaderLength = headerLength;
        }
    }

    public static class FakeRpcTcpFrame
    {
        public static FakeRpcTcpFrame<TBody> Decode<TBody>(byte[] bytes) where TBody : class
        {
            var span = bytes.AsSpan();

            // TotalLength
            var bytesOfTotalLength = span.Slice(0, 4);
            var totalLength = BitConverter.ToInt32(bytesOfTotalLength);

            // HeaderLength
            var bytesOfHeaderLengtth = span.Slice(4, 4);
            var headerLength = BitConverter.ToInt32(bytesOfHeaderLengtth);

            // Header
            var bytesOfHeader = span.Slice(8, headerLength);
            var payloadOfHeader = Encoding.UTF8.GetString(bytesOfHeader);
            var header = JsonConvert.DeserializeObject<Dictionary<string, string>>(payloadOfHeader);

            if (!header.ContainsKey(Constants.FAKE_RPC_HEADER_CONTENT_TYPE))
                throw new ArgumentException($"The header of tcp frame must contains key \"{Constants.FAKE_RPC_HEADER_CONTENT_TYPE}\" to indicate the serialization protocol.");

            var contentType = header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE];
            var messageSerializer = MessageSerializerFactory.Create(contentType);

            // Body
            var bytesOfBody = span.Slice(8 + headerLength, totalLength - (4 + headerLength)).ToArray();
            var body = messageSerializer.Deserialize<TBody>(bytesOfBody);

            return new FakeRpcTcpFrame<TBody>(body, header, totalLength, headerLength);
        }

        public static byte[] Encode<TBody>(FakeRpcTcpFrame<TBody> tcpFrame, string contentType) where TBody : class
        {
            if (tcpFrame.Header == null)
                tcpFrame.Header = new Dictionary<string, string>();

            tcpFrame.Header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] = contentType;

            // Header
            var payloadOfHeader = JsonConvert.SerializeObject(tcpFrame.Header);
            var bytesOfHeader = Encoding.UTF8.GetBytes(payloadOfHeader);

            // HeaderLength
            var bytesOfHeaderLength = BitConverter.GetBytes(bytesOfHeader.Length);

            // Body
            var messageSerializer = MessageSerializerFactory.Create(contentType);
            var bytesOfBody = messageSerializer.Serialize(tcpFrame.Body);

            // TotalLength
            var bytesOfTotalLength = BitConverter.GetBytes(bytesOfBody.Length + bytesOfHeader.Length + 4);

            return bytesOfTotalLength.Concat(bytesOfHeaderLength).Concat(bytesOfHeader).Concat(bytesOfBody).ToArray();
        }

        public static bool Validate(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return false;

            var span = bytes.AsSpan();

            // TcpFrame must have TotalLength
            // TotalLength = bytes.Length - 4
            try
            {
                var totalLength = BitConverter.ToInt32(span.Slice(0, 4));
                if (totalLength != bytes.Length - 4)
                    return false;
            }
            catch
            {
                return false;
            }

            // TcpFrame must have TotalLength and HeaderLength
            if (bytes.Length < 8) return false;

            try
            {
                var headerLength = BitConverter.ToInt32(span.Slice(4, 4));

                // TcpFrame must have Header
                var bytesOfHeader = span.Slice(8, headerLength);
                var payloadOfHeader = Encoding.UTF8.GetString(bytesOfHeader);
                var header = JsonConvert.DeserializeObject<Dictionary<string, string>>(payloadOfHeader);

                // The Header of TcpFrame must contains key "Content-Type"
                if (header == null || header.Count == 0) return false;
                if (!header.ContainsKey(Constants.FAKE_RPC_HEADER_CONTENT_TYPE)) return false;

                // The value of "Content-Type" should be "application/json" or "application/x-msgpack" or "application/x-protobuf"
                var contentType = header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE];
                var supportedContentTypes = FakeRpcContentTypes.SupportedContentTypes();
                if (!supportedContentTypes.Contains(contentType)) return false;
            }
            catch
            {
                return false;
            }


            return true;
        }
    }
}
