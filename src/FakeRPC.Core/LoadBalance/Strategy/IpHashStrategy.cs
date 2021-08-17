using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FakeRpc.Core.LoadBalance.Strategy
{
    public class IpHashStrategy : ILoadBalanceStrategy
    {
        private string _ipAddress;

        public IpHashStrategy(string ipAddress)
        {
            _ipAddress = ipAddress ?? GetLocalIP();
        }

        public TElement Select<TElement>(IEnumerable<TElement> elements) where TElement : ServiceRegistration
        {
            int hashCode = Math.Abs(_ipAddress.GetHashCode());
            int index = hashCode % elements.Count();
            return elements.ElementAt(index);
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
