using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Tcp
{
    public class FakeRpcRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("jsonrpc")]
        public string Version { get; set; } = "2.0";

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("serviceGroup")]
        public string ServiceGroup { get; set; }

        [JsonProperty("methodName")]
        public string MethodName { get; set; }

        [JsonProperty("mathodParams")]
        public KeyValuePair<string, object>[] MethodParams { get; set; }
    }
}
