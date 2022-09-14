using FakeRpc.Core;
using FakeRpc.Core.Serialize;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FakeRpc.Test
{
    public class SerializationTest
    {
        [Fact]
        public async Task Test_ParseResponseOfProtobuf()
        {
            // Arrange
            var response = new FakeRpcResponse();
            response.Id = Guid.NewGuid().ToString("N");
            response.SetResult(new { Type = "RPC", Name = "FakeRPC" });

            // Act
            var serializer = new ProtobufSerializer();
            var bytes = await serializer.SerializeAsync(response);
            var newResponse = await serializer.DeserializeAsync<FakeRpcResponse>(bytes);

            // Assert
            Assert.True(newResponse.Id == response.Id);
        }

        [Fact]
        public async Task Test_ParseRequestOfProtobuf()
        {
            // Arrange
            var request = new FakeRpcRequest();
            request.Id = Guid.NewGuid().ToString("N");
            request.ServiceName = "SayHello";

            // Act
            var serializer = new ProtobufSerializer();
            var bytes = await serializer.SerializeAsync(request);
            var newRequest = await serializer.DeserializeAsync<FakeRpcRequest>(bytes);

            // Assert
            Assert.True(newRequest.Id == request.Id);
            Assert.True(newRequest.ServiceName == request.ServiceName);
        }

        [Fact]
        public async Task Test_ParseResponseOfMessagePack()
        {
            // Arrange
            var response = new FakeRpcResponse();
            response.Id = Guid.NewGuid().ToString("N");
            response.SetResult(new { Type = "RPC", Name = "FakeRPC" });

            // Act
            var serializer = new Core.Serialize.MessagePackSerializer();
            var bytes = await serializer.SerializeAsync(response);
            var newResponse = await serializer.DeserializeAsync<FakeRpcResponse>(bytes);

            // Assert
            Assert.True(newResponse.Id == response.Id);
        }

        [Fact]
        public async Task Test_ParseRequestOfMessagePack()
        {
            // Arrange
            var request = new FakeRpcRequest();
            request.Id = Guid.NewGuid().ToString("N");
            request.ServiceName = "SayHello";

            // Act
            var serializer = new Core.Serialize.MessagePackSerializer();
            var bytes = await serializer.SerializeAsync(request);
            var newRequest = await serializer.DeserializeAsync<FakeRpcRequest>(bytes);

            // Assert
            Assert.True(newRequest.Id == request.Id);
            Assert.True(newRequest.ServiceName == request.ServiceName);
        }

    }
}

