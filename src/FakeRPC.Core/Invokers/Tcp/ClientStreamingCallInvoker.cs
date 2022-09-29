using FakeRpc.Core.Mics;
using FakeRpc.Core.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Tcp
{
    public class ClientStreamingCallInvoker : IClientStreamingCallInvoker
    {
        private readonly TcpClient _tcpClient;

        private readonly IServiceProvider _serviceProvider;

        private readonly IMessageSerializer _messageSerializer;

        public EventHandler<FakeRpcRequest> OnMessageSent { get; set; }

        public EventHandler<FakeRpcResponse> OnMessageReceived { get; set; }

        private readonly byte[] _buffer = new byte[Constants.FAKE_RPC_MAX_BUFFER_SIZE];

        public ClientStreamingCallInvoker(IServiceProvider serviceProvider, TcpClient tcpClient, IMessageSerializer messageSerializer)
        {
            _tcpClient = tcpClient;
            _serviceProvider = serviceProvider;
            _messageSerializer = messageSerializer;
        }

        public async Task InvokeAsync(FakeRpcRequest fakeRpcRequest)
        {
            using (var stream = _tcpClient.GetStream())
            {
                await WriteFrmme(stream, fakeRpcRequest).ContinueWith(async t =>
                {
                    await ReadFrame(stream, fakeRpcRequest);
                });
            }
        }

        private Dictionary<string, string> BuildTcpFrameHeader()
        {
            var header = new Dictionary<string, string>();

            if (_messageSerializer is DefaultSerializer)
            {
                header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] = FakeRpcContentTypes.Default;
            }
            else if (_messageSerializer is Serialize.MessagePackSerializer)
            {
                header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] = FakeRpcContentTypes.MessagePack;
            }
            else if (_messageSerializer is ProtobufSerializer)
            {
                header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] = FakeRpcContentTypes.Protobuf;
            }
            else if (_messageSerializer is FlatSharpSerializer)
            {
                header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] = FakeRpcContentTypes.FlatBuffer;
            }

            return header;
        }

        private async Task WriteFrmme(NetworkStream stream, FakeRpcRequest fakeRpcRequest)
        {
            var header = BuildTcpFrameHeader();
            var tcpFrame = new FakeRpcTcpFrame<FakeRpcRequest>() { Body = fakeRpcRequest, Header = header };
            var payload = FakeRpcTcpFrame.Encode<FakeRpcRequest>(tcpFrame, header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE]);
            await stream.WriteAsync(payload);
            OnMessageSent?.Invoke(this, fakeRpcRequest);
        }

        private async Task ReadFrame(NetworkStream stream, FakeRpcRequest fakeRpcRequest)
        {
            var receivedBytes = new List<byte>();
            var receivedLength = 0;

            do
            {
                var length = await stream.ReadAsync(_buffer, receivedLength, _buffer.Length);
                if (length == 0) break;
                receivedBytes.AddRange(_buffer.AsMemory().Slice(0, length).ToArray());
                receivedLength += length;
                if (receivedLength > Constants.FAKE_RPC_MAX_BUFFER_SIZE)
                {
                    var error = string.Format(Constants.FAKE_RPC_WEBSOCKET_MESSAGE_TOO_BIG, receivedBytes.Count, Constants.FAKE_RPC_MAX_BUFFER_SIZE);
                    OnMessageReceived?.Invoke(this, new FakeRpcResponse() { Id = fakeRpcRequest.Id, Error = error });
                    break;
                }
            } while (stream.DataAvailable);

            var bytes = receivedBytes.ToArray();
            if (!FakeRpcTcpFrame.Validate(bytes))
            {
                // todo: 
            }

            var tcpFrame = FakeRpcTcpFrame.Decode<FakeRpcResponse>(bytes);
            OnMessageReceived?.Invoke(this, tcpFrame.Body);
        }
    }
}
