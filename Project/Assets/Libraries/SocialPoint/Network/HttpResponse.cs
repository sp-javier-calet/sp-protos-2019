using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System;

using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class HttpResponse
    {
        public const string ContentLengthHeader = "Content-Length";
        public const string ContentEncodingHeader = "Content-Encoding";
        const string CompressGzipContentEncoding = "gzip";
        const string CompressDeflateContentEncoding = "deflate";
        public const string LastModifiedHeader = "Last-Modified";
        public const string LastModifiedHeaderFormat = "dddd, dd MMMM yyyy HH:mm:ss tt";

        public enum StatusCodeType
        {
            Success = 200,
            NotModified = 304,
            BadRequestError = 400,
            UnauthorizedError = 401,
            PaymentRequiredError = 402,
            ForbiddenError = 403,
            NotFound = 404,
            TimeOutError = 408,
            CancelledError = 409,
            /* custom errors not specified in HTTP standard */
            UnknownError = 470,
            ValidationError = 471,
            BadResponseError = 474,
            ConnectionFailedError = 475,
            SSLError = 476,
            NotAvailableError = 477,
        };

        public int StatusCode
        {
            get;
            set;
        }

        public Error Error { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public byte[] OriginalBody;
        private byte[] _body;
        private bool _bodyLoaded = false;

        public byte[] Body
        {
            get
            {
                if(!_bodyLoaded)
                {
                    if(!HasHeader(ContentEncodingHeader))
                    {
                        _body = OriginalBody;
                    }
                    else
                    {
                        var encoding = Headers[ContentEncodingHeader];
                        _body = HttpEncoding.Decode(OriginalBody, encoding);
                    }
                    _bodyLoaded = true;
                }
                return _body;
            }
        }

        public HttpResponse(int code = 0)
        {
            Error = new Error();
            Headers = new Dictionary<string, string>();
            StatusCode = code;
        }

        public HttpResponse(int code, Dictionary<string, string> headers) : this(code)
        {
            Headers = headers;
        }

        public bool HasError
        {
            get
            {
                return StatusCode < 200 || StatusCode >= 400;
            }
        }

        public bool HasConnectionError
        {
            get
            {
                return StatusCode == (int)StatusCodeType.TimeOutError || StatusCode == (int)StatusCodeType.ConnectionFailedError || StatusCode == (int)StatusCodeType.NotAvailableError;
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

        public string LastModified
        {
            get
            {
                if(!HasHeader(LastModifiedHeader))
                {
                    return null;
                }
                string modified = Headers[LastModifiedHeader];

                DateTime time = Convert.ToDateTime(modified);
                return time.ToString(LastModifiedHeaderFormat);
            }
        }

        const string kHeaderSeparator = ": ";
        const string kNewline = "\n";

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

        public override string ToString()
        {
            var str = new StringBuilder();

            str.Append(StatusCode);
            str.Append(kNewline);

            if(Headers.Count > 0)
            {
                str.Append(ToStringHeaders());
                str.Append(kNewline);
            }

            if(Body.Length > 0)
            {
                str.Append(new ArraySegment<byte>(Body, 0, 100));
                str.Append(kNewline);
            }

            str.Append(kNewline);

            if(Error != null && Error.HasError)
            {
                str.Append(Error);
                str.Append(kNewline);
            }

            return str.ToString();
        }

        #region QualityStats required Properties

        public double DownloadSize
        {
            get;
            set;
        }

        public double DownloadSpeed
        {
            get;
            set;
        }

        public double Duration
        {
            get
            {
                return ConnectionDuration + TransferDuration;
            }
            set
            {
            }
        }

        public double ConnectionDuration
        {
            get;
            set;
        }

        public double TransferDuration
        {
            get;
            set;
        }

        #endregion
    }
}
