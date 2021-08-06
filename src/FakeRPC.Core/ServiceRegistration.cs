using System;
using System.Collections.Generic;
using System.Text;

namespace FakeRpc.Core
{
    [Serializable]
    public class ServiceRegistration
    {
        /// <summary>
        /// ServiceId
        /// </summary>
        public Guid ServiceId { get; set; }

        /// <summary>
        /// ServiceUri
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// ServiceName
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// ServiceGroup
        /// </summary>
        public string ServiceGroup { get; set; }

        /// <summary>
        /// ExtraData
        /// </summary>
        public Dictionary<string,string> ExtraData { get; set; }


        public void AddExtraData(string key, string value)
        {
            if (ExtraData == null) 
                ExtraData = new Dictionary<string, string>();

            ExtraData[key] = value;
        }
    }
}
