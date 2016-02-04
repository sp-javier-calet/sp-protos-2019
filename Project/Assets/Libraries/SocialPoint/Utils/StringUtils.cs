using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

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
                else if(!string.IsNullOrEmpty(part[0]))
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

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        
        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public const char WildcardMultiChar = '*';
        public const string WildcardDeep = "**";
        public const char WildcardOneChar = '?';

        
        static public bool IsWildcard(string path)
        {
            var i = path.IndexOfAny(new char[]{ WildcardOneChar, WildcardMultiChar });
            return i != -1;
        }

        const string DefaultJoinSeparator = ", ";

        public static string Join<T>(IEnumerable<T> objs, string sep=null)
        {
            if(objs == null)
            {
                return string.Empty;
            }
            if(sep == null)
            {
                sep = DefaultJoinSeparator;
            }
            var strs = new List<string>();
            foreach(var obj in objs)
            {
                if(obj != null)
                {
                    strs.Add(obj.ToString());
                }
            }
            return string.Join(sep, strs.ToArray());
        }

        private const char UriSeparator = '/';

        public static string FixBaseUri(string uri)
        {
            if(uri == null)
            {
                return null;
            }
            // Ensure the URL always contains a trailing slash
            if(!uri.EndsWith(UriSeparator.ToString()))
            {
                uri += UriSeparator;
            }
            return uri;
        }

        public static string CombineUri(string baseUri, string relUri)
        {
            return FixBaseUri(baseUri) + relUri.TrimStart(UriSeparator);
        }

        public static  bool GlobMatch(string pattern, string value)
        {
            bool deep = pattern.Contains(WildcardDeep);
            if(deep)
            {
                pattern = pattern.Replace(WildcardDeep, WildcardMultiChar.ToString());
            }
            else if(value.Split(Path.DirectorySeparatorChar).Length != pattern.Split(Path.DirectorySeparatorChar).Length)
            {
                return false;
            }
            
            int pos = 0;
            while (pattern.Length != pos)
            {
                switch (pattern[pos])
                {
                case WildcardOneChar:
                    break;
                    
                case WildcardMultiChar:
                    for (int i = value.Length; i >= pos; i--)
                    {
                        if(GlobMatch(pattern.Substring(pos + 1), value.Substring(i)))
                        {
                            return true;
                        }
                    }
                    return false;
                    
                default:
                    if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                    {
                        return false;
                    }
                    break;
                }
                
                pos++;
            }
            return value.Length == pos;
        }
    }
}
