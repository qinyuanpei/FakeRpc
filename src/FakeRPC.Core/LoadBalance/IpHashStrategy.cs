using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FakeRpc.Core.LoadBalance
{
    public class IpHashStrategy : ILoadBalanceStrategy
    {
        private string _ipAddress;

        public IpHashStrategy(string ipAddress)
        {
            _ipAddress = ipAddress ?? GetLocalIP();
        }

        public TUpstreamInfo Select<TUpstreamInfo>(IEnumerable<TUpstreamInfo> upstreamInfos)
        {
            int hashCode = Math.Abs(_ipAddress.GetHashCode());
            int index = hashCode % upstreamInfos.Count();
            return upstreamInfos.ElementAt(index);
        }

        private string GetLocalIP()
        {
            var hostName = Dns.GetHostName();
            var ipEntry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < ipEntry.AddressList.Length; i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipEntry.AddressList[i].ToString();
                }
            }

            return "127.0.0.1";
        }
    }
}
