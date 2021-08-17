using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.Core.LoadBalance.Strategy
{
    public class RoundRobinWithWeightStrategry : ILoadBalanceStrategy
    {
        private int _index = 0;
        private readonly static object _lockObj = new object();

        public TElement Select<TElement>(IEnumerable<TElement> elements) where TElement : ServiceRegistration
        {
            var list = new List<TElement>();
            foreach (var element in elements)
            {
                for (var i = 0; i < element.ServiceWeight; i++)
                {
                    list.Add(element);
                }
            }

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
