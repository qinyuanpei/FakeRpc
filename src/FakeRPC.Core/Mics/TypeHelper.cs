using DevExpress.Xpo;
using System;
using System.Linq;
using System.Reflection;

namespace FakeRpc.Core.Mics
{
    public static class TypeHelper
    {
        public static string GetServiceName(this Type type)
        {
            if (!type.IsInterface)
                type = type.GetInterfaces().FirstOrDefault(x => x.GetCustomAttribute<FakeRpcAttribute>() != null);

            var fakeRpcAttr = type.GetCustomAttribute<FakeRpcAttribute>();
            if (fakeRpcAttr != null && !string.IsNullOrEmpty(fakeRpcAttr.ServiceGroup))
                return fakeRpcAttr.ServiceName;

            return type.Name.AsSpan().Slice(1).ToString();
        }

        public static string GetServiceGroup(this Type type)
        {
            if (!type.IsInterface)
                type = type.GetInterfaces().FirstOrDefault(x => x.GetCustomAttribute<FakeRpcAttribute>() != null);

            var fakeRpcAttr = type.GetCustomAttribute<FakeRpcAttribute>();
            if (fakeRpcAttr != null && !string.IsNullOrEmpty(fakeRpcAttr.ServiceGroup))
                return fakeRpcAttr.ServiceGroup;

            return type.Assembly.GetName().Name;
        }

        public static string GetServiceRoute(this Type type, string actionName)
        {
            var serviceName = type.GetServiceName();
            var serviceGroup = type.GetServiceGroup().Replace(".", "/");
            return $"{Constants.FAKE_RPC_ROUTE_PREFIX}/{serviceGroup}/{serviceName}/{actionName}";
        }

        public static string GetClientName(this Type type)
        {
            var serviceName = type.GetServiceName();
            var serviceGroup = type.GetServiceGroup();
            return $"{serviceGroup }.{serviceName}";
        }
    }

}