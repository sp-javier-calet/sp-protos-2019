using System.Collections.Generic;
using UnityEngine;
using System;
using SocialPoint.Utils;
using SPDebug = SocialPoint.Base.Debug;
using System.Collections;

namespace SocialPoint.Network
{
    public class CurlHttpClient  : BaseYieldHttpClient, IDisposable
    {
        static int _initCount = 0;

        public CurlHttpClient(MonoBehaviour mono) : base(mono)
        {
            Init();
        }

        ~CurlHttpClient()
        {
            Dispose();
        }

        void Init()
        {
            if(_initCount == 0)
            {
                CurlBridge.SPUnityCurlInit();
            }
            _initCount++;
        }

        public void Dispose()
        {
            _initCount--;
            if(_initCount <= 0)
            {
                CurlBridge.SPUnityCurlDestroy();
            }
        }

        public override void OnApplicationPause(bool pause)
        {
            if(pause)
            {
                if(Current != null)
                {
                    IEnumerator e = Current.Update();
                    e.MoveNext();
                }
            }
            CurlBridge.SPUnityCurlOnApplicationPause(pause);
        }

        protected override BaseYieldHttpConnection CreateConnection(HttpRequest req, HttpResponseDelegate del)
        {
            if(_initCount == 0)
            {
                Init();
            }
            int id = CurlBridge.SPUnityCurlCreateConn();
            return new CurlHttpConnection(id, req, del);
        }
    }
}

