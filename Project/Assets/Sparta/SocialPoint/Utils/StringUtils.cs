using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace SocialPoint.Utils
{
    public static class StringBuilderExtension
    {
        public static void Append(this StringBuilder builder, params string[] strList)
        {
            for(int i = 0, count = strList.Length; i < count; ++i)
            {
                builder.Append(strList[i]);
            }
        }

        public static bool IsNullOrEmpty(this StringBuilder builder)
        {
            return builder.Length == 0;
        }
    }

    public class StringUtils
    {
        static Stack<StringBuilder> _builders;
        const int _buildersMaxSize = 10;

        public static StringBuilder StartBuilder()
        {
            if(_builders == null)
            {
                _builders = new Stack<StringBuilder>();
            }
            if(_builders.Count == 0)
            {
                return new StringBuilder();
            }
            else
            {
                var builder = _builders.Pop();
                builder.Length = 0;
                return builder;
            }
        }

        public static string FinishBuilder(StringBuilder builder)
        {
            if(_builders == null)
            {
                _builders = new Stack<StringBuilder>();
            }
            var str = builder.ToString();
            builder.Length = 0;
            if(_builders.Count < _buildersMaxSize)
            {
                _builders.Push(builder);
            }
            return str;
        }

        public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format = "")
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format; 

            var sb = StartBuilder();
            for(int k = 0; k < items.Count(); k++)
            {
                KeyValuePair<T,V> item = items.ElementAt(k);
                sb.AppendFormat(format, item.Key, item.Value);
            }
            
            return sb.ToString();
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
            for(int i = 0; i < parts.Length; ++i)
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

        public static string Join<T>(IEnumerable<T> objs, string sep = null)
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
            while(pattern.Length != pos)
            {
                switch(pattern[pos])
                {
                case WildcardOneChar:
                    break;
                    
                case WildcardMultiChar:
                    for(int i = value.Length; i >= pos; i--)
                    {
                        if(GlobMatch(pattern.Substring(pos + 1), value.Substring(i)))
                        {
                            return true;
                        }
                    }
                    return false;
                    
                default:
                    if(value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                    {
                        return false;
                    }
                    break;
                }
                
                pos++;
            }
            return value.Length == pos;
        }

        public static bool EndsWith(string a, string b)
        {
            int ap = a.Length - 1;
            int bp = b.Length - 1;
            while(ap >= 0 && bp >= 0 && a[ap] == b[bp])
            {
                ap--;
                bp--;
            }
            return (bp < 0 && a.Length >= b.Length) || (ap < 0 && b.Length >= a.Length);

        }

        public static bool StartsWith(string a, string b)
        {
            int aLen = a.Length;
            int bLen = b.Length;
            int ap = 0;
            int bp = 0;
            while(ap < aLen && bp < bLen && a[ap] == b[bp])
            {
                ap++;
                bp++;
            }
            return (bp == bLen && aLen >= bLen) || (ap == aLen && bLen >= aLen);
        }
    }
}
