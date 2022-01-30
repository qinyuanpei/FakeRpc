using FakeRpc.Core.Mics;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FakeRpc.Core.Tcp
{
    public class FakeRpcConnectionHandler : ConnectionHandler
    {
        private readonly IFakeRpcMessageParser _messageParser;

        private readonly IServiceProvider _serviceProvider;

        private IList<TypeInfo> _serviceTypes;

        public FakeRpcConnectionHandler(IFakeRpcMessageParser messageParser, IServiceProvider serviceProvider)
        {
            _messageParser = messageParser;
            _serviceProvider = serviceProvider;

        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            _serviceTypes = CollectServiceTypes();

            while (true)
            {
                var result = await connection.Transport.Input.ReadAsync();
                var buffer = result.Buffer;

                var rpcResponse = new FakeRpcResponse<object>();

                if (_messageParser.TryParseMessage<FakeRpcRequest>(buffer, out FakeRpcRequest rpcRequest))
                {
                    var serviceType = _serviceTypes.FirstOrDefault(x => x.GetServiceName() == rpcRequest.ServiceName && x.GetServiceGroup() == rpcRequest.ServiceGroup);
                    if (serviceType == null)
                    {
                        rpcResponse.Id = rpcRequest.Id;
                        rpcResponse.Error = FakeRpcError.ServiceNotFound;
                        rpcResponse.Result = null;
                        await connection.Transport.Output.WriteAsync(rpcResponse.ToByteArray());
                        break;
                    }

                    var serviceInstance = _serviceProvider.GetService(serviceType);
                    if (serviceInstance == null)
                    {
                        rpcResponse = new FakeRpcResponse<object>();
                        rpcResponse.Id = rpcRequest.Id;
                        rpcResponse.Error = FakeRpcError.ServiceNotFound;
                        rpcResponse.Result = null;
                        await connection.Transport.Output.WriteAsync(rpcResponse.ToByteArray());
                        break;
                    }

                    var serviceMethod = serviceInstance.GetType().GetMethod(rpcRequest.MethodName);
                    if (serviceMethod == null)
                    {
                        rpcResponse = new FakeRpcResponse<object>();
                        rpcResponse.Id = rpcRequest.Id;
                        rpcResponse.Error = FakeRpcError.MethodNotFound;
                        rpcResponse.Result = null;
                        await connection.Transport.Output.WriteAsync(rpcResponse.ToByteArray());
                        break;
                    }


                    rpcResponse = new FakeRpcResponse<object>();
                    rpcResponse.Id = rpcRequest.Id;
                    rpcResponse.Error = null;
                    rpcResponse.Result = new { Message = "Hello 飞鸿踏雪" };
                    await connection.Transport.Output.WriteAsync(rpcResponse.ToByteArray());

                    connection.Transport.Input.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }

        private IList<TypeInfo> CollectServiceTypes()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var feferdAssemblies = entryAssembly.GetReferencedAssemblies().Select(x => Assembly.Load(x.Name));
            var allAssemblies = new List<Assembly> { entryAssembly }.Concat(feferdAssemblies);
            return allAssemblies.SelectMany(x => x.DefinedTypes).Where(x => x.IsInterface && x.GetCustomAttribute<FakeRpcAttribute>() != null).ToList();
        }
    }
}
