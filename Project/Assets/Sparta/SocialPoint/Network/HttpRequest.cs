using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Attributes;

namespace SocialPoint.Network
{
    public enum HttpRequestPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }

    public sealed class HttpRequest
    {
        const char AcceptEncodingSeparator = ',';
        public const string AcceptHeader = "Accept";
        public const string AcceptEncodingHeader = "Accept-Encoding";
        public const string ContentTypeHeader = "Content-Type";
        public const string ContentEncodingHeader = "Content-Encoding";
        public const string ContentTypeUrlencoded = "application/x-www-form-urlencoded";
        public const string ContentTypeJson = "application/json";

        public enum MethodType
        {
            GET,
            POST,
            PUT,
            HEAD,
            DELETE
        }

        Uri _url;
        byte[] _body;
        AttrDic _queryParams;
        AttrDic _bodyParams;

        public string BodyEncoding;

        public HttpRequestPriority Priority;

        public MethodType Method;

        public Dictionary<string, string> Headers;

        public float Timeout;

        public float ActivityTimeout;

        public string Proxy;

        public Uri Url
        {
            get
            {
                return _url;
            }

            set
            {
                _url = value;
                _queryParams = null;
            }
        }

        public byte[] Body
        {
            get
            {
                return _body;
            }

            set
            {
                _body = value;
                _bodyParams = null;
            }
        }

        public bool ParamsInBody
        {
            get
            {
                if(Method != MethodType.POST)
                {
                    return false;
                }
                if(Body == null || Body.Length == 0)
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
                return ParamsInBody ? BodyParams : QueryParams;
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
                if(_queryParams == null && Url != null && Url.Query != null)
                {
                    _queryParams = new UrlQueryAttrParser().ParseString(Url.Query).AsDic;
                }
                return _queryParams;
            }

            set
            {
                if(Url == null)
                {
                    Url = new Uri("///");
                }
                var builder = new UriBuilder(Url);
                builder.Query = new UrlQueryAttrSerializer().SerializeString(value);
                Url = builder.Uri;
                _queryParams = value;
            }
        }

        public AttrDic BodyParams
        {
            get
            {
                if(_bodyParams == null && Body != null)
                {
                    _bodyParams = new UrlQueryAttrParser().Parse(Body).AsDic;
                }
                return _bodyParams;
            }
            
            set
            {
                Body = value == null ? null : new UrlQueryAttrSerializer().Serialize(value);
                if(!HasHeader(ContentTypeHeader))
                {
                    AddHeader(ContentTypeHeader, ContentTypeUrlencoded);
                }
                _bodyParams = value;
            }
        }

        public List<string> AcceptEncodings
        {
            get
            {
                return !HasHeader(AcceptEncodingHeader) ? null : new List<string>(Headers[AcceptEncodingHeader].Split(AcceptEncodingSeparator));
            }
        }

        public bool CompressBody
        {
            set
            {
                BodyEncoding = value ? HttpEncoding.DefaultBodyCompression : null;
            }

            get
            {
                return HttpEncoding.IsCompressed(BodyEncoding);
            }
        }

        public bool AcceptCompressed
        {
            set
            {
                var parts = AcceptEncodings ?? new List<string>();

                for(int i = 0, HttpEncodingCompressedEncodingsLength = HttpEncoding.CompressedEncodings.Length; i < HttpEncodingCompressedEncodingsLength; i++)
                {
                    var part = HttpEncoding.CompressedEncodings[i];
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

                parts.RemoveAll(str => str == null || str.Trim().Length == 0);

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
                for(int i = 0, HttpEncodingCompressedEncodingsLength = HttpEncoding.CompressedEncodings.Length; i < HttpEncodingCompressedEncodingsLength; i++)
                {
                    var part = HttpEncoding.CompressedEncodings[i];
                    if(parts.Contains(part))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public HttpRequest()
        {
            Priority = HttpRequestPriority.Normal;
            Headers = new Dictionary<string, string>();
            Timeout = 0.0f;
            ActivityTimeout = 0.0f;
            AcceptCompressed = true;
        }

        public HttpRequest(Attr data) : this()
        {
            FromAttr(data);
        }

        public HttpRequest(HttpRequest other) : this()
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

        public void BeforeSend()
        {
            Body = HttpEncoding.Encode(Body, BodyEncoding);
            if(!string.IsNullOrEmpty(BodyEncoding) && !HasHeader(ContentEncodingHeader))
            {
                AddHeader(ContentEncodingHeader, BodyEncoding);
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

        public string GetHeader(string key)
        {
            string value;
            if(Headers.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        public void RemoveHeader(string key)
        {
            Headers.Remove(key);
        }

        public void AddParam(string key, Attr value)
        {
            var parms = Params ?? new AttrDic();
            parms[key] = value;
            Params = parms;
        }

        public void AddParam(string key, string value)
        {
            var parms = Params ?? new AttrDic();
            parms.SetValue(key, value);
            Params = parms;
        }

        public bool HasParam(string key)
        {
            var parms = Params;
            return parms != null && parms.ContainsKey(key);
        }

        public void RemoveParam(string key)
        {
            var parms = Params;
            if(parms != null)
            {
                parms.Remove(key);
                Params = parms;
            }
        }

        public void RemoveQueryParam(string key)
        {
            var parms = QueryParams;
            if(parms != null)
            {
                parms.Remove(key);
                QueryParams = parms;
            }
        }

        public void AddQueryParam(string key, Attr value)
        {
            var parms = QueryParams ?? new AttrDic();
            parms[key] = value;
            QueryParams = parms;
        }

        public void AddQueryParam(string key, string value)
        {
            var parms = QueryParams ?? new AttrDic();
            parms.SetValue(key, value);
            QueryParams = parms;
        }

        public bool HasQueryParam(string key)
        {
            var parms = QueryParams;
            return parms != null && parms.ContainsKey(key);
        }

        public void AddBodyParam(string key, Attr value)
        {
            var parms = BodyParams ?? new AttrDic();
            parms[key] = value;
            BodyParams = parms;
        }

        public void AddBodyParam(string key, string value)
        {
            var parms = BodyParams ?? new AttrDic();
            parms.SetValue(key, value);
            BodyParams = parms;
        }

        public bool HasBodyParam(string key)
        {
            var parms = BodyParams;
            return parms != null && parms.ContainsKey(key);
        }

        public void RemoveBodyParam(string key)
        {
            var parms = BodyParams;
            if(parms != null)
            {
                parms.Remove(key);
                BodyParams = parms;
            }
        }

        const string kHeaderSeparator = ": ";
        const string kLineSeparator = " ";
        const string kNewline = "\n";

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append(Method);
            str.Append(kLineSeparator);
            if(Url != null)
            {
                str.Append(Url);
            }
            else
            {
                str.Append("[null]");
            }
            str.Append(kNewline);

            if(Headers.Count > 0)
            {
                str.Append(ToStringHeaders());
                str.Append(kNewline);
            }

            if(Body != null && Body.Length > 0)
            {
                str.Append(Body);
                str.Append(kNewline);
            }
            return str.ToString();
        }

        public string ToStringHeaders()
        {
            var str = new StringBuilder();

            var itr = Headers.GetEnumerator();
            while(itr.MoveNext())
            {
                var data = itr.Current;
                str.Append(data.Key);
                str.Append(kHeaderSeparator);
                str.Append(data.Value);
                str.Append(kNewline);
            }
            itr.Dispose();

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
            itr.Dispose();
            if(Body != null)
            {
                data.SetValue(AttrKeyBody, Convert.ToBase64String(Body));
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
            var itr = dataDic.Get(AttrKeyHeaders).AsDic.GetEnumerator();
            while(itr.MoveNext())
            {
                var header = itr.Current;
                AddHeader(header.Key, header.Value.AsValue.ToString());
            }
            itr.Dispose();

            Body = null;
            if(dataDic.ContainsKey(AttrKeyBody))
            {
                Body = Convert.FromBase64String(dataDic.Get(AttrKeyBody).AsValue.ToString());
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
