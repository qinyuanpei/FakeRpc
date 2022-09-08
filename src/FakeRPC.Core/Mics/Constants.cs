using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.Mics
{
    public class Constants
    {
        public const string FAKE_RPC_ROUTE_PREFIX = "rpc";

        public const string FAKE_RPC_HTTP_METHOD = "POST";

        public const string FAKE_RPC_SERVICE_GROUP = "ServiceGroup";

        public const string FAKE_RPC_SERVICE_SCHEMA = "ServiceSchema";

        public const string FAKE_RPC_SERVICE_PROVIDER = "ServiceProvider";

        public const string FAKE_RPC_SERVICE_INTERFACE = "ServiceInterface";

        public const string FAKE_RPC_SERVICE_PROTOCOLS = "ServiceProtocols";

        public const string FAKE_RPC_SERVICE_SCHEMA_HTTP = "http";

        public const string FAKE_RPC_SERVICE_SCHEMA_HTTPS = "https";

        public const int FAKE_RPC_WEBSOCKET_MAX_BUFFER_SIZE = 64 * 1024;

        public const string FAKE_RPC_WEBSOCKET_MESSAGE_TOO_BIG = "The message exceeds the maximum allowed message size: {0} of allowed {1} bytes.";
    }
}
