using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeRpc.Core.LoadBalance.Strategy
{
    public class RandomWithWeightStrategy : ILoadBalanceStrategy
    {
        private readonly Random random = new Random();

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

            var index = random.Next(0, elements.Count());
            return list.ElementAt(index);
        }
    }
}