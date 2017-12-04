using System;

namespace SocialPoint.Network
{
    public class RetryHttpClient : IHttpClient
    {
        readonly IHttpClient _client;

        public int MaxRetries = 2;

        public Func<HttpResponse, bool> ShouldRetry;

        public Action<HttpRequest, HttpResponse> RetryFailed;

        public RetryHttpClient(IHttpClient client)
        {
            _client = client;
        }

        public event HttpRequestDelegate RequestSetup;

        public IHttpConnection Send(HttpRequest req, HttpResponseDelegate del = null)
        {
            if(RequestSetup != null)
            {
                RequestSetup(req);
            }
            return _client.Send(req, resp => OnResponse(0, req, resp, del));
        }

        void OnResponse(int retries, HttpRequest req, HttpResponse resp, HttpResponseDelegate del)
        {
            if(retries > MaxRetries)
            {
                if(del != null)
                {
                    del(resp);
                }
                if(RetryFailed != null)
                {
                    RetryFailed(req, resp);
                }
                return;
            }
            var shouldRetry = false;
            if(ShouldRetry != null)
            {
                shouldRetry = ShouldRetry(resp);
            }
            else if(resp.HasRecoverableError)
            {
                shouldRetry = true;
            }
            if(!shouldRetry)
            {
                if(del != null)
                {
                    del(resp);
                }
                return;
            }
            _client.Send(req, resp2 => OnResponse(retries+1, req, resp2, del));
        }

        public string Config
        {
            set
            {
                _client.Config = value;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}