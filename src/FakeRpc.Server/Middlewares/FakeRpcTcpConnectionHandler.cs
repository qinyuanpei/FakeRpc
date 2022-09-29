using FakeRpc.Core;
using FakeRpc.Core.Invokers.Tcp;
using FakeRpc.Core.Mics;
using Microsoft.AspNetCore.Connections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Server.Middlewares
{
    public class FakeRpcTcpConnectionHandler : ConnectionHandler
    {
        private IServerStreamingCallInvoker _callInvoker;

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            while (true)
            {
                using (var stream = new MemoryStream())
                {
                    // 读取请求
                    var result = await connection.Transport.Input.ReadAsync();

                    do
                    {
                        await stream.WriteAsync(result.Buffer.ToArray());
                        result = await connection.Transport.Input.ReadAsync();
                    } while (!result.IsCompleted);

                    // 处理请求
                    await _callInvoker.InvokeAsync(stream);

                    // 写入响应
                    var buffer = new byte[Constants.FAKE_RPC_MAX_BUFFER_SIZE];
                    var receivedLength = await stream.ReadAsync(buffer);
                    await connection.Transport.Output.WriteAsync(buffer.AsMemory().Slice(0, receivedLength));
                }
            }
        }
    }
}
