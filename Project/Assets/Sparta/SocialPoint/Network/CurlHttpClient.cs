
using System;
using System.Collections;
using System.Collections.Generic;
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
                    _appEvents.WillGoBackground.Add(-1000, OnWillGoBackground);
                    _appEvents.WasOnBackground += WasOnBackground;
                }
            }
        }

        void DisconnectAppEvents()
        {
            if(_appEvents != null)
            {
                _appEvents.WillGoBackground.Remove(OnWillGoBackground);
                _appEvents.WasOnBackground -= WasOnBackground;
            }
        }

        public string Config
        {
            set
            {
                CurlBridge.SPUnityCurlSetConfig(value);
            }
        }

        public CurlHttpClient(ICoroutineRunner runner) : base(runner)
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
            int id = CurlBridge.SPUnityCurlCreateConn();
            return new CurlHttpConnection(id, req, del);
        }
    }
}

