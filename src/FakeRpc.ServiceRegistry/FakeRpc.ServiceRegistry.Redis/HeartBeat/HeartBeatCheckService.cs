using CSRedis;
using FakeRpc.Core;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Registry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FakeRpc.ServiceRegistry.Redis
{
    public class RedisHeartBeatCheckService : IHostedService
    {
        private Timer _timer;
        private readonly TcpClient _tcpClient = new TcpClient();
        private readonly CSRedisClient _redisClient;
        private readonly IServiceRegistry _serviceRegistry;
        private readonly RedisServiceRegistryOptions _options;
        private ILogger<RedisHeartBeatCheckService> _logger;
        public RedisHeartBeatCheckService(
            RedisServiceRegistryOptions options, 
            IServiceRegistry serviceRegistry,
            ILogger<RedisHeartBeatCheckService> logger)
        {
            _options = options;
            _logger = logger;
            _redisClient = new CSRedisClient(options.RedisUrl);
            _serviceRegistry = serviceRegistry;
            RedisHelper.Initialization(_redisClient);
            _tcpClient.SendTimeout = 10 * 60 * 1000;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(RedisHeartBeatCheckService)} start running....");
            _timer = new Timer(DoCheck, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(RedisHeartBeatCheckService)} stop running....");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void DoCheck(object state)
        {
            var keys = _redisClient.Keys($"{Constants.FAKE_RPC_ROUTE_PREFIX}:*");
            foreach(var key in keys)
            {
                var serviceNodes = _redisClient.SMembers<ServiceRegistration>(key);
                if (serviceNodes.Any())
                {
                    foreach(var serviceNode in serviceNodes)
                    {
                        try
                        {
                            serviceNode.ServiceUri = new Uri("http://127.0.0.1:5003");
                            _tcpClient.Connect(serviceNode.ServiceHost, serviceNode.ServicePort);
                            if (_tcpClient.Connected)
                            {
                                _logger.LogInformation($"Node {serviceNode.ServiceHost}:{serviceNode.ServicePort} is unhealthy ...");
                                //_serviceRegistry.Unregister(serviceNode);
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.LogInformation($"Node {serviceNode.ServiceHost}:{serviceNode.ServicePort} is unhealthy ...");
                            //_serviceRegistry.Unregister(serviceNode);
                            continue;
                        }

                        _logger.LogInformation($"Node {serviceNode.ServiceHost}:{serviceNode.ServicePort} is healthy ...");
                    }
                }
            }
        }
    }
}
