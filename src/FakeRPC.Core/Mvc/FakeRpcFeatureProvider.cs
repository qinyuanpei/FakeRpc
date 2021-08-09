using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FakeRpc.Core.Mvc
{
    public class FakeRpcFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            // Implementation Type
            var type = typeInfo.AsType();
            if (type.IsInterface) return false;

            // Interface Type
            var interfaces = type.GetInterfaces();
            if (interfaces != null && interfaces.Any())
                type = interfaces[0];

            var fakeRpc = type.GetCustomAttribute<FakeRpcAttribute>(true);
            return fakeRpc != null;
        }
    }
}
