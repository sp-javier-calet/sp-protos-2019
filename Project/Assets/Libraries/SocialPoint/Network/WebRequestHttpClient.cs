using System;
using System.Net;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class WebRequestHttpClient : BaseYieldHttpClient
    {

        public WebRequestHttpClient(ICoroutineRunner runner) : base(runner)
        {
        }

        private HttpWebRequest ConvertRequest(HttpRequest req)
        {
            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(req.Url);

            wreq.Timeout = (int)req.Timeout * 1000; // Value is in milliseconds
            wreq.ReadWriteTimeout = (int)req.ActivityTimeout * 1000; // Value is in milliseconds

            if(req.Headers != null)
            {
                foreach(var pair in req.Headers)
                {
                    switch(pair.Key)
                    {
                    case "Accept":
                        wreq.Accept = pair.Value;
                        break;
                    case "Connection":
                        wreq.Connection = pair.Value;
                        break;
                    case "Content-Length":
                        long num = 0;
                        long.TryParse(pair.Value, out num);
                        wreq.ContentLength = num;
                        break;
                    case "Content-Type":
                        wreq.ContentType = pair.Value;
                        break;
                    case "Expect":
                        wreq.Expect = pair.Value;
                        break;
                    case "If-Modified-Since":
                        DateTime date = new DateTime();
                        DateTime.TryParse(pair.Value, out date);
                        wreq.IfModifiedSince = date;
                        break;
                    case "Referer":
                        wreq.Referer = pair.Value;
                        break;
                    case "Transfer-Encoding":
                        wreq.TransferEncoding = pair.Value;
                        break;
                    case "User-Agent":
                        wreq.UserAgent = pair.Value;
                        break;
                    case "Date":
                    case "Host":
                    case "Range":
                    case "Proxy-Connection":
                        LogUnsupportedHeader(pair.Key);
                        break;
                    default:
                        if(WebHeaderCollection.IsRestricted(pair.Key))
                        {
                            LogUnsupportedHeader(pair.Key);
                        }
                        else
                        {
                            wreq.Headers.Add(pair.Key, pair.Value);
                        }
                        break;
                    }
                }
            }

            if(string.IsNullOrEmpty(wreq.ContentType))
            {
                wreq.ContentType = HttpRequest.ContentTypeUrlencoded;
            }
            wreq.Method = req.Method.ToString();

            if(!string.IsNullOrEmpty(req.Proxy))
            {
                wreq.Proxy = new System.Net.WebProxy(req.Proxy);
            }
            else
            {
                wreq.Proxy = WebRequest.DefaultWebProxy;
            }

            return wreq;
        }

        private void LogUnsupportedHeader(string name)
        {
            DebugUtils.Log(string.Format("HttpWebRequest does not support the '{0}' header", name));
        }

        protected override BaseYieldHttpConnection CreateConnection(HttpRequest req, HttpResponseDelegate del)
        {
            return new WebRequestHttpConnection(ConvertRequest(req), del, req.Body);
        }

    }
}
