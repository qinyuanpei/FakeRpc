using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace FakeRpc.Core.Mvc
{
    public class FakeRpcProtocolsProvider
    {
        private readonly IOptionsSnapshot<MvcOptions> _options;
        public FakeRpcProtocolsProvider(IOptionsSnapshot<MvcOptions> options)
        {
            _options = options;
        }

        public IEnumerable<string> GetProtocols()
        {
            var protocols = new List<string>() { "application/json" };

            if (_options.Value.FormatterMappings.GetMediaTypeMappingForFormat("msgpack") != null)
                protocols.Add(FakeRpcContentTypes.MessagePack);

            if (_options.Value.FormatterMappings.GetMediaTypeMappingForFormat("protobuf") != null)
                protocols.Add(FakeRpcContentTypes.Protobuf);

            return protocols;
        }
    }

}