using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Network
{
    public abstract class BaseYieldHttpConnection : IHttpConnection
    {
        public abstract IEnumerator Update();
        public abstract void Cancel();

        HttpResponseDelegate _callback;

        public BaseYieldHttpConnection(HttpResponseDelegate callback)
        {
            _callback = callback;
        }

        protected void OnResponse(HttpResponse resp)
        {
            if(_callback != null)
            {
                try
                {
                    _callback(resp);
                }
                catch(Exception e)
                {
                    DebugUtils.LogException(e);
                }
            }
        }

        public void Release()
        {
            _callback = null;
        }
    }

    public abstract class BaseYieldHttpClient : IHttpClient
    {
        protected BaseYieldHttpConnection Current = null;
        protected PriorityQueue<HttpRequestPriority, BaseYieldHttpConnection> Pending;
        ICoroutineRunner _runner = null;
        IEnumerator _updateCoroutine = null;
        
        public BaseYieldHttpClient(ICoroutineRunner runner)
        {
            _runner = runner;
            Pending = new PriorityQueue<HttpRequestPriority, BaseYieldHttpConnection>();
        }
        
        protected abstract BaseYieldHttpConnection CreateConnection(HttpRequest req, HttpResponseDelegate del);
    
        private string _defaultProxy;
        /// <summary>
        ///     Default proxy address that will be set for any connection created using
        ///     this client.
        ///     Set it to String.Emtpy or null to disable the proxy.
        /// </summary>
        [Obsolete("Please use the RequestSetup event to set default request values")]
        public string DefaultProxy
        {
            set
            {
                _defaultProxy = value;
            }
        }

        /// <summary>
        ///     Will be called on all requests before sending them
        /// </summary>
        public event HttpRequestDelegate RequestSetup;

        protected virtual void SetupHttpRequest(HttpRequest req)
        {
            if(string.IsNullOrEmpty(req.Proxy))
            {
                req.Proxy = _defaultProxy;
            }
            if(RequestSetup != null)
            {
                RequestSetup(req);
            }
        }

        public virtual void Dispose()
        {
            foreach(var connection in Pending)
            {
                connection.Cancel();
            }
            _runner.StopCoroutine(_updateCoroutine);
        }

        public virtual IHttpConnection Send(HttpRequest req, HttpResponseDelegate del = null)
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
            var conn = CreateConnection(req, del);
            Pending.Add(req.Priority, conn);
            if(_updateCoroutine == null)
            {
                _updateCoroutine = Update();
                _runner.StartCoroutine(_updateCoroutine);
            }
            return conn;
        }
        
        public virtual IEnumerator Update()
        {
            while(Pending.Count > 0)
            {
                Current = Pending.Remove();
                var e = Current.Update();
                while(e != null && e.MoveNext() && Current != null)
                {
                    yield return e.Current;
                }
                Current = null;
            }
            _updateCoroutine = null;
        }
    }
}
