using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeRpc.Core.LoadBalance.Strategy
{
    public class RoundRobinStrategy : ILoadBalanceStrategy
    {
        private int _index = 0;
        private readonly static object _lockObj = new object();

        public TElement Select<TElement>(IEnumerable<TElement> elements) where TElement : ServiceRegistration
        {
            lock (_lockObj)
            {
                if (_index >= elements.Count())
                    _index = 0;

                var element = elements.ElementAt(_index);
                _index++;

                return element;
            }
        }
    }
}