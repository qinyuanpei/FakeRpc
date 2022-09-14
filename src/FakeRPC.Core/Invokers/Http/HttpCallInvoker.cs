using FakeRpc.Core.Binder.Http;
using FakeRpc.Core.Serialize;
using MessagePack;
using ProtoBuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Invokers.Http
{
    public class HttpCallInvoker : IHtttpCallInvoker
    {
        private readonly HttpClient _httpClient;

        private readonly IMessageSerializer _serializer;

        public HttpCallInvoker(HttpClient httpClient, IMessageSerializer serializer)
        {
            _httpClient = httpClient;
            _serializer = serializer;
        }

        public async Task<TResponse> CallAsync<TRequest, TResponse>(Uri uri, TRequest request)
        {
            var payload = await _serializer.SerializeAsync(request);
            var httpContent = new ByteArrayContent(payload);
            SetHttpHeaders(httpContent);
            var response = await _httpClient.PostAsync(uri, httpContent);
            payload = await response.Content.ReadAsByteArrayAsync();
            return await _serializer.DeserializeAsync<TResponse>(payload);
        }

        public Task<TResponse> CallAsync<TResponse>(Uri uri)
        {
            if (_serializer is DefaultSerializer)
            {
                return CallAsync<object, TResponse>(uri, new { });
            }
            else if (_serializer is Serialize.MessagePackSerializer)
            {
                return CallAsync<object, TResponse>(uri, Nil.Default);
            }
            else if (_serializer is ProtobufSerializer)
            {
                return CallAsync<object, TResponse>(uri, new Empty());
            }

            throw new ArgumentException($"The specified serializer {_serializer.GetType().FullName} does not support.");
        }

        private void SetHttpHeaders(HttpContent httpContent)
        {
            if (_serializer is DefaultSerializer)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(FakeRpcContentTypes.Default));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(FakeRpcContentTypes.Default);
            }
            else if (_serializer is Serialize.MessagePackSerializer)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(FakeRpcContentTypes.MessagePack));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(FakeRpcContentTypes.MessagePack);
            }
            else if (_serializer is ProtobufSerializer)
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(FakeRpcContentTypes.Protobuf));
                httpContent.Headers.ContentType = new MediaTypeHeaderValue(FakeRpcContentTypes.Protobuf);
            }
        }
    }
}
