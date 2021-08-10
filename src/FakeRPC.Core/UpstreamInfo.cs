using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    public class UpstreamInfo : ServiceRegistration
    {
        public int Weight { get; set; }
    }
}
