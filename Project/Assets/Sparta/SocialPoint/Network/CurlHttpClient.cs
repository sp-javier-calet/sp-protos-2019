using System.Collections;
using SocialPoint.Utils;
using SocialPoint.AppEvents;

namespace SocialPoint.Network
{
    public class CurlHttpClient  : BaseYieldHttpClient
    {
        static int _initCount = 0;
        IAppEvents _appEvents;

        readonly Curl _curl;

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

        public override string Config
        {
            set
            {
                _curl.SetConfig(value);
            }
        }

        public CurlHttpClient(ICoroutineRunner runner) : base(runner)
        {
            if(_initCount == 0)
            {
                _curl = new Curl(false);
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
                _curl.Dispose();
            }
        }

        void OnWillGoBackground()
        {
            if(Current != null)
            {
                IEnumerator e = Current.Update();
                e.MoveNext();
            }
            _curl.Pause = true;
        }

        void WasOnBackground()
        {
            _curl.Pause = false;
        }

        protected override BaseYieldHttpConnection CreateConnection(HttpRequest req, HttpResponseDelegate del)
        {
            
            var conn = _curl.CreateConnection();
            return new CurlHttpConnection(conn, req, del);
        }
    }
}

