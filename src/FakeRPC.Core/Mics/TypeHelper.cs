using DevExpress.Xpo;
using System;
using System.Reflection;

namespace FakeRpc.Core.Mics
{
    public static class TypeHelper
    {
        public static string GetServiceName(this Type type)
        {
            var fakeRpcAttr = type.GetCustomAttribute<FakeRpcAttribute>(true);
            if (fakeRpcAttr != null && !string.IsNullOrEmpty(fakeRpcAttr.ServiceGroup))
                return fakeRpcAttr.ServiceName;

            if (!type.IsInterface)
                return type.Name;

            return type.Name.AsSpan().Slice(1).ToString();
        }

        public static string GetServiceGroup(this Type type)
        {
            var fakeRpcAttr = type.GetCustomAttribute<FakeRpcAttribute>(true);
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
    }

}