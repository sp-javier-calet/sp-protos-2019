using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public abstract class BaseYieldHttpConnection : Petition
    {
        public abstract IEnumerator Update();
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
            Pending = new PriorityQueue<HttpRequestPriority, BaseYieldHttpConnection>(new HttpRequest.PriorityComparer());
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
            RequestSetup(req);
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        public void CancelAllPetitions()
        {
            foreach(BaseYieldHttpConnection connection in Pending.All)
            {
                connection.Cancel();
            }
        }

        public virtual Petition Send(HttpRequest req, HttpResponseDelegate del = null)
        {
            SetupHttpRequest(req);
            req.BeforeSend();
            BaseYieldHttpConnection conn = CreateConnection(req, del);

            Pending.Enqueue(req.Priority, conn);
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
                Current = Pending.Dequeue();
                IEnumerator e = Current.Update();
                while(e != null && e.MoveNext() && Current != null && Current.Active)
                {
                    yield return e.Current;
                }
                Current = null;
            }
			_update = null;
        }
    }
}
