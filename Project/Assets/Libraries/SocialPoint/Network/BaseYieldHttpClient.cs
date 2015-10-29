using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SocialPoint.Utils;

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
                _callback(resp);
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
        MonoBehaviour _behaviour = null;
        Coroutine _update = null;
        
        public BaseYieldHttpClient(MonoBehaviour behaviour)
        {
            _behaviour = behaviour;
            // Initialize queues for all available priorities
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
        }

        public virtual IHttpConnection Send(HttpRequest req, HttpResponseDelegate del = null)
        {
            SetupHttpRequest(req);
            req.BeforeSend();
            var conn = CreateConnection(req, del);
            Pending.Add(req.Priority, conn);
            if(_update == null)
            {
                _update = _behaviour.StartCoroutine(Update());
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
            _update = null;
        }
    }
}
