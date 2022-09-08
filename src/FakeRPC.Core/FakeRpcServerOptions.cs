using FakeRpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FakeRpc.Core
{
    public class FakeRpcServerOptions
    {
        /// <summary>
        /// Host of FakeRpc Server
        /// </summary>
        public string ServerHost { get; set; }

        /// <summary>
        /// Port of FakeRpc Server
        /// </summary>
        public string ServerPort { get; set; }

        /// <summary>
        /// Registered Services
        /// </summary>
        public IEnumerable<FakeRpcServiceDescriptor> ServiceDescriptors => ResolveServiceDescriptors();

        /// <summary>
        /// Registered Assembies
        /// </summary>
        public IList<Assembly> ExternalAssemblies { get; set; } = new List<Assembly>();

        private IEnumerable<FakeRpcServiceDescriptor> ResolveServiceDescriptors()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var referedAssemblies = entryAssembly.GetReferencedAssemblies().Select(x => Assembly.Load(x));
            var mergedAssemblies = new List<Assembly> { entryAssembly }.Concat(referedAssemblies).Concat(ExternalAssemblies);

            var definedTypes = mergedAssemblies.SelectMany(x => x.DefinedTypes).Select(x => x.AsType()).Distinct();
            var serviceTypes = definedTypes.Where(x => x.IsInterface && x.GetCustomAttribute<FakeRpcAttribute>() != null);

            foreach (var serviceType in serviceTypes)
            {
                var implType = definedTypes.FirstOrDefault(x => serviceType.IsAssignableFrom(x));
                if (implType != null)
                    yield return new FakeRpcServiceDescriptor() { ServiceType = serviceType, ImplementationType = implType };
            }
        }
    }
}
