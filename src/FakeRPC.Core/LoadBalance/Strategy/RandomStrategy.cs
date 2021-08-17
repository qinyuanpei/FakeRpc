using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.Core.LoadBalance.Strategy
{
    public class RandomStrategy : ILoadBalanceStrategy
    {
        private readonly Random random = new Random();

        public TElement Select<TElement>(IEnumerable<TElement> elements) where TElement : ServiceRegistration
        {
            var index = random.Next(0, elements.Count());
            return elements.ElementAt(index);
        }
    }
}
