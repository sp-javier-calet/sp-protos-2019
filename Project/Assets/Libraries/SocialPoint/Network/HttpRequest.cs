using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Attributes;

namespace SocialPoint.Network
{
    public enum HttpRequestPriority
    {
        Low     = 0,
        Normal  = 1,
        High    = 2
    }
    ;

    public class HttpRequest
    {
        const char AcceptEncodingSeparator = ',';
        public const string AcceptHeader = "Accept";
        public const string AcceptEncodingHeader = "Accept-Encoding";
        public const string ContentTypeHeader = "Content-Type";
        public const string ContentEncodingHeader = "Content-Encoding";
        public const string ContentTypeUrlencoded = "application/x-www-form-urlencoded";
        public const string ContentTypeJson = "application/json";

        public static DataCompression DefaultBodyCompression = DataCompression.Gzip;

        public class PriorityComparer : Comparer<HttpRequestPriority>
        {
            public override int Compare(HttpRequestPriority first, HttpRequestPriority second)
            {
                return second - first;
            }
        };

        public enum MethodType
        {
            GET,    
            POST,
            PUT,
            HEAD,
            DELETE
        }
        ;


        public DataCompression BodyCompression;

        public HttpRequestPriority Priority;

        public Uri Url;

        public MethodType Method;

        public Dictionary<string, string> Headers;

        public Data Body;

        public float Timeout;

        public float ActivityTimeout;

        public string Proxy;

        public bool ParamsInBody
        {
            get
            {
                if(Method != MethodType.POST)
                {
                    return false;
                }
                if(Body.Length == 0)
                {
                    return true;
                }
                if(!HasHeader(ContentTypeHeader))
                {
                    return false;
                }
                return Headers[ContentTypeHeader] == ContentTypeUrlencoded;
            }
        }

        public AttrDic Params
        {
            get
            {
                if(ParamsInBody)
                {
                    return BodyParams;
                }
                else
                {
                    return QueryParams;
                }
            }
            
            set
            {
                if(ParamsInBody)
                {
                    BodyParams = value;
                }
                else
                {
                    QueryParams = value;
                }
            }
        }

        public AttrDic QueryParams
        {
            get
            {
                if(Url == null || Url.Query == null)
                {
                    return null;
                }
                return new UrlQueryAttrParser().Parse(new Data(Url.Query)).AsDic;
            }

            set
            {
                var builder = new UriBuilder(Url);
                builder.Query = new UrlQueryAttrSerializer().Serialize(value).ToString();
                Url = builder.Uri;
            }
        }
        
        public AttrDic BodyParams
        {
            get
            {
                return new UrlQueryAttrParser().Parse(Body).AsDic;
            }
            
            set
            {
                Body = new UrlQueryAttrSerializer().Serialize(value);
                if(!HasHeader(ContentTypeHeader))
                {
                    AddHeader(ContentTypeHeader, ContentTypeUrlencoded);
                }
            }
        }

        public Dictionary<string,string> FlatParams
        {
            get
            {
                if(Url == null || Url.Query == null)
                {
                    return null;
                }
                return StringUtils.QueryToDictionary(Url.Query);
            }
        }

        public HttpRequest()
        {
            BodyCompression = DataCompression.None;
            Priority = HttpRequestPriority.Normal;
            Headers = new Dictionary<string, string>();
            Timeout = 0.0f;
            ActivityTimeout = 0.0f;
            AcceptCompressed = true;
        }

        public HttpRequest(Attr data): this()
        {
            FromAttr(data);
        }

        public HttpRequest(HttpRequest other): this()
        {
            FromAttr(other.ToAttr());
            Priority = other.Priority;
        }

        public HttpRequest(Uri url, MethodType method = MethodType.GET) : this()
        {
            Url = url;
            Method = method;
        }

        public HttpRequest(string url, MethodType method = MethodType.GET) : this(new Uri(url), method)
        {
        }

        public List<string> AcceptEncodings
        {
            get
            {
                if(!HasHeader(AcceptEncodingHeader))
                {
                    return null;
                }
                return new List<string>(Headers[AcceptEncodingHeader].Split(AcceptEncodingSeparator));
            }
        }

        public bool CompressBody
        {
            set
            {
                if(value)
                {
                    BodyCompression = DefaultBodyCompression;
                }
                else
                {
                    BodyCompression = DataCompression.None;
                }
            }

            get
            {
                return BodyCompression != DataCompression.None;
            }
        }

        public void BeforeSend()
        {

            Body = Body.Compress(BodyCompression);
            var encoding = BodyCompression.ToEncoding();
            if(!string.IsNullOrEmpty(encoding) && !HasHeader(ContentEncodingHeader))
            {
                AddHeader(ContentEncodingHeader, encoding);
            }
            if(Timeout == 0.0f)
            {
                Timeout = 60.0f;
            }
        }

        public bool AcceptCompressed
        {
            set
            {
                var parts = AcceptEncodings;
                if(parts == null)
                {
                    parts = new List<string>();
                }

                foreach(var part in DataCompressionExtension.Encodings)
                {
                    if(value)
                    {
                        if(!parts.Contains(part))
                        {
                            parts.Add(part);
                        }
                    }
                    else
                    {
                        parts.Remove(part);
                    }
                }

                parts.RemoveAll(str => {
                    return str == null || str.Trim().Length == 0;
                });

                if(parts.Count == 0)
                {
                    RemoveHeader(AcceptEncodingHeader);
                }
                else
                {
                    Headers[AcceptEncodingHeader] = String.Join(AcceptEncodingSeparator.ToString(), parts.ToArray());
                }
            }

            get
            {
                var parts = AcceptEncodings;
                if(parts == null)
                {
                    return false;
                }
                foreach(var part in DataCompressionExtension.Encodings)
                {
                    if(parts.Contains(part))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void AddHeader(string key, string value)
        {
            Headers.Add(key, value);
        }

        public bool HasHeader(string key)
        {
            return Headers.ContainsKey(key) && Headers[key] != null;
        }
        
        public void RemoveHeader(string key)
        {
            Headers.Remove(key);
        }

        public void AddParam(string key, Attr value)
        {
            var parms = Params;
            parms[key] = value;
            Params = parms;
        }

        public void AddParam(string key, string value)
        {
            var parms = Params;
            parms.SetValue(key, value);
            Params = parms;
        }

        public bool HasParam(string key)
        {
            return Params.ContainsKey(key);
        }

        public void RemoveQueryParam(string key)
        {
            var parms = QueryParams;
            parms.Remove(key);
            QueryParams = parms;
        }
        
        public void AddQueryParam(string key, Attr value)
        {
            var parms = QueryParams;
            parms[key] = value;
            QueryParams = parms;
        }
        
        public void AddQueryParam(string key, string value)
        {
            var parms = QueryParams;
            parms.SetValue(key, value);
            QueryParams = parms;
        }
        
        public bool HasQueryParam(string key)
        {
            return QueryParams.ContainsKey(key);
        }
        
        public void RemoveParam(string key)
        {
            var parms = QueryParams;
            parms.Remove(key);
            QueryParams = parms;
        }

        public void AddBodyParam(string key, Attr value)
        {
            var parms = BodyParams;
            parms[key] = value;
            QueryParams = parms;
        }
        
        public void AddBodyParam(string key, string value)
        {
            var parms = BodyParams;
            parms.SetValue(key, value);
            QueryParams = parms;
        }
        
        public bool HasBodyParam(string key)
        {
            return BodyParams.ContainsKey(key);
        }
        
        public void RemoveBodyParam(string key)
        {
            var parms = BodyParams;
            parms.Remove(key);
            QueryParams = parms;
        }

        const string kHeaderSeparator = ": ";
        const string kLineSeparator = " ";
        const string kNewline = "\n";

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append(Method);
            str.Append(kLineSeparator);
            str.Append(Url);
            str.Append(kNewline);

            if(Headers.Count > 0)
            {
                str.Append(ToStringHeaders());
                str.Append(kNewline);
            }

            if(Body.Length > 0)
            {
                str.Append(Body);
                str.Append(kNewline);
            }
            return str.ToString();
        }

        public string ToStringHeaders()
        {
            var str = new StringBuilder();

            foreach(KeyValuePair<string, string> data in Headers)
            {
                str.Append(data.Key);
                str.Append(kHeaderSeparator);
                str.Append(data.Value);
                str.Append(kNewline);
            }

            return str.ToString();
        }

        const string AttrKeyUrl = "url";
        const string AttrKeyMethod = "method";
        const string AttrKeyHeaders = "headers";
        const string AttrKeyBody = "body";
        const string AttrKeyTimeout = "timeout";
        const string AttrKeyActivityTimeout = "activity_timeout";
        const string AttrKeyProxy = "proxy";

        public Attr ToAttr()
        {
            var data = new AttrDic();
            if(Url != null)
            {
                data.SetValue(AttrKeyUrl, Url.ToString());
            }
            data.SetValue(AttrKeyMethod, Method.ToString());
            var hdrs = new AttrDic();
            data.Set(AttrKeyHeaders, hdrs);
            var itr = Headers.GetEnumerator();
            while(itr.MoveNext())
            {
                hdrs.SetValue(itr.Current.Key, itr.Current.Value);
            }
            if(Body.Bytes != null)
            {
                data.SetValue(AttrKeyBody, Convert.ToBase64String(Body.Bytes));
            }
            data.SetValue(AttrKeyTimeout, Timeout);
            data.SetValue(AttrKeyActivityTimeout, ActivityTimeout);
            if(Proxy != null)
            {
                data.SetValue(AttrKeyProxy, Proxy);
            }

            return data;
        }

        public void FromAttr(Attr data)
        {
            var dataDic = data.AsDic;
            Url = null;
            if(dataDic.ContainsKey(AttrKeyUrl))
            {
                Url = new Uri(dataDic.Get(AttrKeyUrl).AsValue.ToString());
            }
            Method = (MethodType)Enum.Parse(typeof(MethodType), dataDic.Get(AttrKeyMethod).AsValue.ToString());
            foreach(var header in dataDic.Get(AttrKeyHeaders).AsDic)
            {
                AddHeader(header.Key, header.Value.AsValue.ToString());
            }
            Body = new Data();
            if(dataDic.ContainsKey(AttrKeyBody))
            {
                Body = new Data(Convert.FromBase64String(dataDic.Get(AttrKeyBody).AsValue.ToString()));
            }
            Timeout = dataDic.Get(AttrKeyTimeout).AsValue.ToInt();
            ActivityTimeout = dataDic.Get(AttrKeyActivityTimeout).AsValue.ToInt();
            if(dataDic.ContainsKey(AttrKeyProxy))
            {
                Proxy = dataDic.Get(AttrKeyProxy).AsValue.ToString();
            }
        }
    }

}
