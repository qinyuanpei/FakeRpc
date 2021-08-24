using Consul;
using CSRedis;
using FakeRpc.Core;
using FakeRpc.Core.Discovery;
using FakeRpc.Core.Mics;
using FakeRpc.Core.Mvc;
using FakeRpc.Core.Mvc.MessagePack;
using FakeRpc.Core.Mvc.Protobuf;
using FakeRpc.Core.Registry;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FakeRpc.Server
{
    public class FakeRpcServerBuilder
    {
        private readonly IServiceCollection _services;
        public IServiceCollection Services => _services;

        private FakeRpcServerOptions _options = new FakeRpcServerOptions();
        public FakeRpcServerOptions Options => _options;

        public FakeRpcServerBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public FakeRpcServerBuilder AddFakeRpc()
        {
            var partManager = _services.BuildServiceProvider().GetService<ApplicationPartManager>();
            if (partManager == null)
                throw new InvalidOperationException("请在AddMvc()方法后调用AddFakeRpc()");

            partManager.FeatureProviders.Add(new FakeRpcFeatureProvider());

            _services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);
            _services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            _services.Configure<MvcOptions>(o => o.Conventions.Add(new FakeRpcModelConvention()));

            _services.AddTransient<FakeRpcProtocolsProvider>();

            return this;
        }

        public FakeRpcServerBuilder UseMessagePack(Action<MessagePackSerializerOptions> configure = null)
        {
            var defaultSerializerOptions = MessagePackSerializer.DefaultOptions;
            configure?.Invoke(defaultSerializerOptions);

            _services.Configure<MvcOptions>(options =>
            {
                options.InputFormatters.Add(new MessagePackInputFormatter(defaultSerializerOptions));
                options.OutputFormatters.Add(new MessagePackOutputFormatter(defaultSerializerOptions));
                options.FormatterMappings.SetMediaTypeMappingForFormat("msgpack", MediaTypeHeaderValue.Parse(FakeRpcMediaTypes.MessagePack));
            });

            return this;
        }

        public FakeRpcServerBuilder UseUseProtobuf()
        {
            _services.Configure<MvcOptions>(options =>
            {
                options.InputFormatters.Add(new ProtobufInputFormatter());
                options.OutputFormatters.Add(new ProtobufOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("protobuf", MediaTypeHeaderValue.Parse(FakeRpcMediaTypes.Protobuf));
            });

            return this;
        }

        public FakeRpcServerBuilder EnableServiceRegistry<TServiceRegistry>(Func<IServiceProvider, TServiceRegistry> serviceRegistryFactory = null) where TServiceRegistry : class, IServiceRegistry
        {
            if (serviceRegistryFactory != null)
                _services.AddSingleton<TServiceRegistry>(serviceRegistryFactory);
            else
                _services.AddSingleton<IServiceRegistry, TServiceRegistry>();

            return this;
        }

        public FakeRpcServerBuilder EnableSwagger(Action<SwaggerGenOptions> setupAction = null)
        {
            if (setupAction == null)
                setupAction = BuildDefaultSwaggerGenAction();
            _services.AddSwaggerGen(setupAction);
            return this;
        }

        public FakeRpcServerBuilder AddExternalAssembly(Assembly assembly)
        {
            if (!_options.ExternalAssemblies.Contains(assembly))
                _options.ExternalAssemblies.Add(assembly);

            return this;
        }

        public FakeRpcServerBuilder AddExternalAssembly(string assemblyPath = null)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                assemblyPath = AppDomain.CurrentDomain.BaseDirectory;

            var assemblyFiles = Directory.GetFiles(assemblyPath);
            foreach (var assemblyFile in assemblyFiles)
            {
                var assembly = Assembly.LoadFrom(Path.Combine(assemblyPath, assemblyFile));
                AddExternalAssembly(assembly);
            }

            return this;
        }

        public void Build()
        {
            ConfigAssemblyParts();
            _services.Configure<FakeRpcServerOptions>(x => x = _options);
        }

        private Action<SwaggerGenOptions> BuildDefaultSwaggerGenAction()
        {
            Action<SwaggerGenOptions> setupAction = options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "FakeRpc Services",
                    Version = "v1",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Name = "飞鸿踏雪",
                        Email = "qinyuanpei@163.com",
                        Url = new Uri("https://blog.yuanpei.me"),
                    }
                });

                options.DocInclusionPredicate((a, b) => true);
                var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var commentFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.xml");
                options.IncludeXmlComments(commentFile);
            };

            return setupAction;
        }

        private void ConfigAssemblyParts()
        {
            var mvcBuilder = _services.AddMvc();
            mvcBuilder.ConfigureApplicationPartManager(apm =>
            {
                foreach (var assembly in _options.ExternalAssemblies)
                    apm.ApplicationParts.Add(new AssemblyPart(assembly));
            });
        }
    }
}
