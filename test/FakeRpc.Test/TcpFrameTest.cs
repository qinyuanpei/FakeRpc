using FakeRpc.Core;
using FakeRpc.Core.Invokers.Tcp;
using FakeRpc.Core.Mics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FakeRpc.Test
{
    public class TcpFrameTest
    {
        [Fact]
        public void Test_TcpFrame_EncodeDecode()
        {
            // Arrange
            var tcpFrame = new FakeRpcTcpFrame<FakeRpcRequest>();
            tcpFrame.Body = new FakeRpcRequest() { Id = Guid.NewGuid().ToString(), MethodName = "SayHello" };

            // Act
            var bytes = FakeRpcTcpFrame.Encode(tcpFrame, FakeRpcContentTypes.Default);

            // Assert
            var newTcpFrame = FakeRpcTcpFrame.Decode<FakeRpcRequest>(bytes);
            Assert.True(newTcpFrame.Body.Id == tcpFrame.Body.Id);
            Assert.True(newTcpFrame.Body.MethodName == tcpFrame.Body.MethodName);
            Assert.True(newTcpFrame.Header.ContainsKey(Constants.FAKE_RPC_HEADER_CONTENT_TYPE));
            Assert.True(newTcpFrame.Header[Constants.FAKE_RPC_HEADER_CONTENT_TYPE] == FakeRpcContentTypes.Default);
        }

        [Fact]
        public void Test_TcpFrame_Validate()
        {
            // Arrange
            var tcpFrame = new FakeRpcTcpFrame<FakeRpcRequest>();
            tcpFrame.Body = new FakeRpcRequest() { Id = Guid.NewGuid().ToString(), MethodName = "SayHello" };

            // Act
            var bytes = FakeRpcTcpFrame.Encode(tcpFrame, FakeRpcContentTypes.Default);

            // Assert
            Assert.True(FakeRpcTcpFrame.Validate(bytes));
            Assert.False(FakeRpcTcpFrame.Validate(Encoding.UTF8.GetBytes("Hi")));
        }
    }
}
