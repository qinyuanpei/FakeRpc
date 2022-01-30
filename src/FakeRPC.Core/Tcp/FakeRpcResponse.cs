using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Tcp
{
    public class FakeRpcResponse<TResult>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; } = "2.0";

        [JsonProperty("result")]
        public TResult Result { get; set; }

        [JsonProperty("error")]
        public FakeRpcError Error { get; set; }


        public byte[] ToByteArray()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
