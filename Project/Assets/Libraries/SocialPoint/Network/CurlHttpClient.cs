
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Utils;
using SocialPoint.AppEvents;

namespace SocialPoint.Network
{
    public class CurlHttpClient  : BaseYieldHttpClient
    {
        static int _initCount = 0;

        IAppEvents _appEvents;
        public IAppEvents AppEvents
        {
            set
            {
                DisconnectAppEvents();
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.RegisterWillGoBackground(-1000, OnWillGoBackground);
                    _appEvents.WasOnBackground += WasOnBackground;
                }
            }
        }

        void DisconnectAppEvents()
        {
            if(_appEvents != null)
            {
                _appEvents.UnregisterWillGoBackground(OnWillGoBackground);
                _appEvents.WasOnBackground -= WasOnBackground;
            }
        }

        public CurlHttpClient(MonoBehaviour mono) : base(mono)
        {
            Init();
        }

        void Init()
        {
            if(_initCount == 0)
            {
                CurlBridge.SPUnityCurlInit();
            }
            _initCount++;
        }

        override public void Dispose()
        {
            base.Dispose();
            DisconnectAppEvents();
            _initCount--;
            if(_initCount <= 0)
            {
                CurlBridge.SPUnityCurlDestroy();
            }
        }

        void OnWillGoBackground()
        {
            if(Current != null)
            {
                IEnumerator e = Current.Update();
                e.MoveNext();
            }
            CurlBridge.SPUnityCurlOnApplicationPause(true);
        }

        void WasOnBackground()
        {
            CurlBridge.SPUnityCurlOnApplicationPause(false);
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

