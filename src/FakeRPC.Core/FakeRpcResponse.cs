using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcResponse<TResult>
    {
        public string Id { get; set; }

        public TResult Result { get; set; }

        public string Error { get; set; }

        public byte[] ToByteArray()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
