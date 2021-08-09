using FakeRpc.Core.Mics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FakeRpc.Core
{
    [Serializable]
    public class ServiceRegistration
    {
        /// <summary>
        /// ServiceUri
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// ServiceHost
        /// </summary>
        public string ServiceHost => ServiceUri.Host;

        /// <summary>
        /// ServicePort
        /// </summary>
        public int ServicePort => ServiceUri.Port;

        /// <summary>
        /// ServiceSchema
        /// </summary>
        public string ServiceSchema => ServiceUri.Scheme;

        /// <summary>
        /// ServiceName
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// ServiceGroup
        /// </summary>
        public string ServiceGroup { get; set; }

        /// <summary>
        /// ServiceInterface
        /// </summary>
        public string ServiceInterface { get; set; }

        /// <summary>
        /// ServiceProtocols
        /// </summary>
        public string ServiceProtocols { get; set; }

        /// <summary>
        /// ServiceProvider
        /// </summary>
        public string ServiceProvider { get; set; } = "FakeRpc";

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

        public string GetServiceId()
        {
            var md5 = new MD5CryptoServiceProvider();

            var signature = $"{Constants.FAKE_RPC_ROUTE_PREFIX}/{ServiceGroup.Replace(".", "/")}/{ServiceName}/{ServiceUri.Host}/{ServiceUri.Port}";
            var input = Encoding.Default.GetBytes(signature);
            var output = md5.ComputeHash(input);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }
    }
}
