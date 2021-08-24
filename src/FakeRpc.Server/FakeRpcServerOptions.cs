using FakeRpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FakeRpc.Server
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
        public IEnumerable<Type> ServiceTypes => ResolveServiceTypes();

        /// <summary>
        /// Registered Assembies
        /// </summary>
        public IList<Assembly> ExternalAssemblies { get; set; } = new List<Assembly>();

        private IEnumerable<Type> ResolveServiceTypes()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var referencedAssemblies = entryAssembly.GetReferencedAssemblies().Select(x => Assembly.Load(x));
            var assemblies = new List<Assembly> { entryAssembly }.Concat(referencedAssemblies).Concat(ExternalAssemblies);
            var serviceTypes = assemblies.SelectMany(x => x.DefinedTypes).Select(x => x.AsType()).Distinct();
            serviceTypes = serviceTypes.Where(x => x.IsInterface && x.GetCustomAttribute<FakeRpcAttribute>() != null);
            return serviceTypes;
        }
    }
}
