using FakeRpc.Core;
using MessagePack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeRpc.Example.Interface
{
    /// <summary>
    /// GreetService
    /// </summary>
    public class GreetService : IGreetService
    {
        private readonly ILogger<GreetService> _logger;
        public GreetService(ILogger<GreetService> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// SayHello
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<HelloReply> SayHello(HelloRequest request)
        {
            _logger.LogInformation($"Invoke SayHello() with \"{JsonConvert.SerializeObject(request)}\"...");
            return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}" });
        }

        /// <summary>
        /// SayWho
        /// </summary>
        /// <returns></returns>
        public Task<HelloReply> SayWho()
        {
            _logger.LogInformation("Invoke SayWho() with \"{ }\"...");
            return Task.FromResult(new HelloReply { Message = $"I'm 长安书小妆" });
        }
    }
}
