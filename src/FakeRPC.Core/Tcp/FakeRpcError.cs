using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Tcp
{
    public class FakeRpcError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }

        public static FakeRpcError ServiceNotFound => new FakeRpcError() { Code = 001, Msg = "Service Not Found" };

        public static FakeRpcError MethodNotFound => new FakeRpcError() { Code = 002, Msg = "Method Not Found" };

        public static FakeRpcError MethodCallError => new FakeRpcError() { Code = 003, Msg = "Method Call Error" };
    }
}
