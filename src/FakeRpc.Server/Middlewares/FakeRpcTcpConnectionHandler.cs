using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Server.Middlewares
{
    public class FakeRpcTcpConnectionHandler : ConnectionHandler
    {
        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            return Task.CompletedTask;
        }
    }
}
