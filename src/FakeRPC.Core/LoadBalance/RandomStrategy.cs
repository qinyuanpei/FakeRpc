using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.Core.LoadBalance
{
    public class RandomStrategy : ILoadBalanceStrategy
    {
        private readonly Random random = new Random();

        public TUpstreamInfo Select<TUpstreamInfo>(IEnumerable<TUpstreamInfo> upstreamInfos)
        {
            var index = random.Next(0, upstreamInfos.Count());
            return upstreamInfos.ElementAt(index);
        }
    }
}
