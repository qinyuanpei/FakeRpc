using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerExample;
using FakeRpc.Core;
using System.Net.Http;
using FakeRpc.Example.Interface;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using FakeRpc.Core.Registry;
using CSRedis;
using FakeRpc.Server;
using FakeRpc.ServiceRegistry.Nacos;
using FakeRpc.ServiceRegistry.Consul;
using FakeRpc.ServiceRegistry.Redis;
using System.Net;

namespace ServerExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var builder = new FakeRpcServerBuilder(services);
            builder
                .AddFakeRpc()
                .UseMessagePack()
                .UseProtobuf()
                .EnableSwagger()
                .AddExternalAssembly(typeof(GreetService).Assembly)
                .AddExternalAssembly(typeof(GreetService).Assembly)
                .AddTcpProtocol(8099)
                /*.EnableRedisServiceRegistry(options => options.RedisUrl = "localhost:6379")*/;
                //.EnableNacosServiceRegistry(options => options.ServerAddress = new List<string> { "http://localhost:8848" });
                //.EnableConsulServiceRegistry(options => options.BaseUrl = "http://localhost:8500");
             
            builder.Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseFakeRpc(applicationLifetime);
        }
    }
}
