﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using FakeRpc.Core.Registry;
using FakeRpc.Core.Mvc;
using FakeRpc.Core;
using FakeRpc.Core.Mics;
using Microsoft.Extensions.Options;

namespace FakeRpc.Server
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseFakeRpc(this IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            // Register & Unregister
            applicationLifetime.ApplicationStarted.Register(() => RegisterServices(app.ApplicationServices));
            applicationLifetime.ApplicationStopped.Register(() => UnregisterService(app.ApplicationServices));

            // Swagger & SwaggerUI
            var swaggerProvider = app.ApplicationServices.GetService<Swashbuckle.AspNetCore.Swagger.ISwaggerProvider>();
            if (swaggerProvider != null)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FakeRpc Services v1"));
            }
        }

        private static void RegisterServices(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var options = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>();
                var serviceRegistry = scope.ServiceProvider.GetService<IServiceRegistry>();
                var protocolsProvider = scope.ServiceProvider.GetService<FakeRpcProtocolsProvider>();
                if (serviceRegistry != null)
                {
                    foreach (var serviceType in options.Value.ServiceTypes)
                    {
                        serviceRegistry.Register(new ServiceRegistration()
                        {
                            ServiceUri = new Uri("http://192.168.6.24:5800"),
                            ServiceName = serviceType.GetServiceName(),
                            ServiceGroup = serviceType.GetServiceGroup(),
                            ServiceInterface = serviceType.FullName,
                            ServiceProtocols = string.Join(",", protocolsProvider.GetProtocols())
                        });
                    }
                }
            } 
        }

        private static void UnregisterService(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var options = scope.ServiceProvider.GetService<IOptions<FakeRpcServerOptions>>();
                var serviceRegistry = scope.ServiceProvider.GetService<IServiceRegistry>();
                var protocolsProvider = scope.ServiceProvider.GetService<FakeRpcProtocolsProvider>();
                if (serviceRegistry != null)
                {
                    foreach (var serviceType in options.Value.ServiceTypes)
                    {
                        serviceRegistry.Unregister(new ServiceRegistration()
                        {
                            ServiceUri = new Uri("http://192.168.6.24:5800"),
                            ServiceName = serviceType.GetServiceName(),
                            ServiceGroup = serviceType.GetServiceGroup(),
                            ServiceInterface = serviceType.FullName,
                            ServiceProtocols = string.Join(",", protocolsProvider.GetProtocols())
                        });
                    }
                }
            }
        }
    }
}
