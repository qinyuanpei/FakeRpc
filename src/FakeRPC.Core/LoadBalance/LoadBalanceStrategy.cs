using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core.LoadBalance
{
    public enum LoadBalanceStrategy
    {
        // 随机
        Random = 0,

        // 加权随机
        RandomWithWeight = 1,

        // 轮训
        RoundRobin = 2,

        // 加权轮训
        RoundRobinWithWeight = 3,

        // 哈希一致性
        IpHash = 4
    }
}
