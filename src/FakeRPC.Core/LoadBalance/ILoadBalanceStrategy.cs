using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.LoadBalance
{
    public interface ILoadBalanceStrategy
    {
        TElement Select<TElement>(IEnumerable<TElement> elements) where TElement : ServiceRegistration;
    }
}
