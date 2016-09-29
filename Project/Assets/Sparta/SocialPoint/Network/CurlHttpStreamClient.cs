using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class CurlHttpStreamClient : IHttpStreamClient
    {
        readonly List<CurlHttpStream> Running;
        readonly ICoroutineRunner _runner;
        readonly Curl _curl;

        IEnumerator _updateCoroutine;

        /// <summary>
        ///     Will be called on all requests before sending them
        /// </summary>
        public event HttpStreamRequestDelegate RequestSetup;

        public bool Verbose
        {
            set
            {
                _curl.Verbose = value;
            }
        }

        public CurlHttpStreamClient(ICoroutineRunner runner)
        {
            _runner = runner;
            _curl = new Curl(true);
            Running = new List<CurlHttpStream>();
        }

        public IHttpStream Connect(HttpRequest req, HttpStreamClosedDelegate del = null)
        {
            if(req == null || req.Url == null)
            {
                if(del != null)
                {
                    del(new HttpResponse((int)HttpResponse.StatusCodeType.BadRequestError));
                }
                return null;
            }
            SetupHttpRequest(req);
            req.BeforeSend();
            var stream = CreateStream(req, del);
            Running.Add(stream);
            if(_updateCoroutine == null)
            {
                _updateCoroutine = Update();
                _runner.StartCoroutine(_updateCoroutine);
            }
            return stream;
        }

        CurlHttpStream CreateStream(HttpRequest req, HttpStreamClosedDelegate del)
        {
            var conn = _curl.CreateConnection();
            return new CurlHttpStream(conn, req, del);
        }

        void SetupHttpRequest(HttpRequest req)
        {
            if(RequestSetup != null)
            {
                RequestSetup(req);
            }
        }

        public void Dispose()
        {
            using(var itr = Running.GetEnumerator())
            {
                while(itr.MoveNext())
                {
                    itr.Current.Cancel();
                }
            }
            _runner.StopCoroutine(_updateCoroutine);
        }

        public virtual IEnumerator Update()
        {
            while(Running.Count > 0)
            {
                _curl.Update();

                for(int i = 0; i < Running.Count; ++i)
                {
                    var finished = Running[i].Update();
                    if(finished)
                    {
                        Running.RemoveAt(i--);
                    }
                }
                yield return null;
            }
            _updateCoroutine = null;
        }
    }
}