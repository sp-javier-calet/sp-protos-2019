using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace AssetBundleGraph
{
    public class PlatformDependentInfo
    {
        const string MESSAGE = "message";
        const string TIMESTAMP = "lastUpdated";

        public string Message;
        public DateTime lastUpdated;

        public PlatformDependentInfo(string message)
        {
            Message = message;
            lastUpdated = DateTime.UtcNow;
        }

        public PlatformDependentInfo(Dictionary<string, object> jsonObject)
        {
            Message = jsonObject[MESSAGE] as string;
            lastUpdated = DateTime.ParseExact(jsonObject[TIMESTAMP] as string, ValidatorLog.DATE_FORMAT, CultureInfo.InvariantCulture);
            DateTime.SpecifyKind(lastUpdated, DateTimeKind.Utc);
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            var dict = new Dictionary<string, object>();

            dict[MESSAGE] = Message;
            dict[TIMESTAMP] = lastUpdated.ToString(ValidatorLog.DATE_FORMAT, CultureInfo.InvariantCulture);

            return dict;
        }
    }
}
