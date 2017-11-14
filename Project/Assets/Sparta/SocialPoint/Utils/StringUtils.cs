using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public static int IndexOf(this StringBuilder sb, string value, int startIndex = 0)
        {
            int valueLength = value.Length;
            int maxSearchLength = (sb.Length - valueLength) + 1;

            for(int i = startIndex; i < maxSearchLength; ++i)
            {
                if(sb[i] == value[0])
                {
                    int valueIndex = 1;
                    while((valueIndex < valueLength) && (sb[i + valueIndex] == value[valueIndex]))
                    {
                        ++valueIndex;
                    }

                    if (valueIndex == valueLength)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static string Substring(this StringBuilder sb, int startingIndex, int length)
        {
            char[] charArray = new char[length];
            sb.CopyTo(startingIndex, charArray, 0, length);
            return new string(charArray);
        }
    }

    public static class StringUtils
    {
        static Stack<StringBuilder> _builders;
        const int _buildersMaxSize = 10;

        public static StringBuilder StartBuilder()
        {
            return new StringBuilder();
        }

        public static string FinishBuilder(StringBuilder builder)
        {
            return builder.ToString();
        }

        public static string DictToString<T, V>(IEnumerable<KeyValuePair<T, V>> items, string format = "")
        {
            format = String.IsNullOrEmpty(format) ? "{0}='{1}' " : format; 

            var sb = StartBuilder();
            var itr = items.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                sb.AppendFormat(format, item.Key, item.Value);
            }
            itr.Dispose();
            
            return FinishBuilder(sb);
        }

        const char QuerySeparator = '&';
        const char QueryAssign = '=';

        public static string DictionaryToQuery(IDictionary<string,string> parms)
        {
            string query = string.Empty;
            string sepChar = string.Empty;
            var itr = parms.GetEnumerator();
            while(itr.MoveNext())
            {
                var entry = itr.Current;
                query += sepChar + entry.Key + QueryAssign + entry.Value;
                sepChar = String.Empty + QuerySeparator;
            }
            itr.Dispose();

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

            for(int i = 0, parmsLength = parms.Length; i < parmsLength; i++)
            {
                KeyValuePair<string, string> param = parms[i];
                result += "&" + param.Key + "=" + Uri.EscapeDataString(param.Value);
            }
            return result;
        }

        public static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public const char WildcardMultiChar = '*';
        public const string WildcardDeep = "**";
        public const char WildcardOneChar = '?';

        
        static public bool IsWildcard(string path)
        {
            var i = path.IndexOfAny(new []{ WildcardOneChar, WildcardMultiChar });
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
            var itr = objs.GetEnumerator();
            while(itr.MoveNext())
            {
                var obj = itr.Current;
                if(obj != null)
                {
                    strs.Add(obj.ToString());
                }
            }
            itr.Dispose();

            return string.Join(sep, strs.ToArray());
        }

        const char UriSeparator = '/';

        public static string FixBaseUri(string uri)
        {
            if(uri == null)
            {
                return null;
            }
            // Ensure the URL always contains a trailing slash
            if(!EndsWith(uri, UriSeparator.ToString()))
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

        const char ServerSeparator = ':';

        public static void ParseServer(string input, out string addr, out int port)
        {
            addr = null;
            port = 0;
            if(string.IsNullOrEmpty(input))
            {
                return;
            }
            var parts = input.Split(ServerSeparator);
            if(parts.Length > 0)
            {
                addr = parts[0];
            }
            if(parts.Length > 1)
            {
                int.TryParse(parts[1], out port);
            }
        }
    }
}
