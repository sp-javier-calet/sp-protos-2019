using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace SocialPoint.Utils
{
    public class StringUtils
    {
        public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format = "")
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format; 
            
            StringBuilder itemString = new StringBuilder();

            for(int k = 0; k < items.Count(); k++)
            {
                KeyValuePair<T,V> item = items.ElementAt(k);
                itemString.AppendFormat(format, item.Key, item.Value);
            }
            
            return itemString.ToString(); 
        }

        public static void InitCapacity(ref string str, int capacity)
        {
            str = "";
            while(capacity > 0)
            {
                str += " ";
                capacity--;
            }
        }

        private const char QuerySeparator = '&';
        private const char QueryAssign = '=';

        public static string DictionaryToQuery(IDictionary<string,string> parms)
        {
            string query = string.Empty;
            string sepChar = string.Empty;
            foreach(KeyValuePair<string, string> entry in parms)
            {
                query += sepChar + entry.Key + QueryAssign + entry.Value;
                sepChar = String.Empty + QuerySeparator;
            }
            return query;
        }

        public static Dictionary<string,string> QueryToDictionary(string query)
        {
            var dict = new Dictionary<string, string>();
            if(string.IsNullOrEmpty(query))
            {
                return dict;
            }
            var parts = query.Split(QuerySeparator);
            for(int i=0; i<parts.Length; ++i)
            {
                var part = parts[i].Split(QueryAssign);
                if(part.Length > 1)
                {
                    dict[Uri.UnescapeDataString(part[0])] = Uri.UnescapeDataString(part[1]);
                }
                else
                {
                    dict[Uri.UnescapeDataString(part[0])] = string.Empty;
                }
            }
            return dict;
        }

        public static string GetIsoTimeStr(DateTime dt)
        {
            return dt.ToString("yyyyMMddTHHmmssZ");
        }
        
        public static string GetIsoTimeStr(string dts)
        {
            DateTime dt = DateTime.ParseExact(dts, "yyyyMMdd", null);
            return GetIsoTimeStr(new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0));
        }
        
        public static string GetJoinedUrlParams(KeyValuePair<string,string>[] parms)
        {
            string result = "";
            foreach(KeyValuePair<string,string> param in parms)
                result += "&" + param.Key + "=" + Uri.EscapeDataString(param.Value);
            return result;
        }
    }
}