using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeRpc.Core.LoadBalance
{
    public class RoundRobinStrategy : ILoadBalanceStrategy
    {
        private int _index = 0;
        private readonly static object _lockObj = new object();

        public TUpstreamInfo Select<TUpstreamInfo>(IEnumerable<TUpstreamInfo> upstreamInfos)
        {
            lock (_lockObj)
            {
                if (_index >= upstreamInfos.Count())
                    _index = 0;

                var upstreamInfo = upstreamInfos.ElementAt(_index);
                _index++;

                return upstreamInfo;
            }
        }
    }
}