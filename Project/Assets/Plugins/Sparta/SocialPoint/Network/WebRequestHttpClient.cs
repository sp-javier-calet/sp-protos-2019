using System;
using System.Collections.Generic;
using System.Net;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class WebRequestHttpClient : BaseYieldHttpClient
    {

        public WebRequestHttpClient(ICoroutineRunner runner) : base(runner)
        {
        }

        public override string Config
        {
            set
            {
            }
        }

        protected override BaseYieldHttpConnection CreateConnection(HttpRequest req, HttpResponseDelegate del)
        {
            return new WebRequestHttpConnection(WebRequestUtils.ConvertRequest(req), del, req.Body);
        }
    }
}
