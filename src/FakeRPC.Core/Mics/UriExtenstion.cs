using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FakeRpc.Core.Mics
{
    public static class UriExtenstion
    {
        public static Dictionary<string, string> GetQueryStrings(this Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.Query))
                return new Dictionary<string, string>();

            return uri.Query.Substring(1)
                    .Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(param => param.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries))
                    .GroupBy(part => part[0], part => part.Length > 1 ? part[1] : string.Empty)
                    .ToDictionary(group => group.Key, group => string.Join(",", group));
        }
    }
}
