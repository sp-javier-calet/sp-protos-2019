using System.Collections;
using SocialPoint.Utils;
using SocialPoint.AppEvents;

namespace SocialPoint.Network
{
    public class CurlHttpClient  : BaseYieldHttpClient
    {
        const int WillGoBackgroundEventPriority = -1000;

        IAppEvents _appEvents;

        protected readonly Curl _curl;

        public IAppEvents AppEvents
        {
            set
            {
                DisconnectAppEvents();
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.WillGoBackground.Add(WillGoBackgroundEventPriority, OnWillGoBackground);
                    _appEvents.WasOnBackground.Add(0, WasOnBackground);
                }
            }
        }

        void DisconnectAppEvents()
        {
            if(_appEvents != null)
            {
                _appEvents.WillGoBackground.Remove(OnWillGoBackground);
                _appEvents.WasOnBackground.Remove(WasOnBackground);
            }
        }

        public override string Config
        {
            set
            {
                _curl.SetConfig(value);
            }
        }

        public CurlHttpClient(ICoroutineRunner runner, bool supportHttp2) : base(runner)
        {
            _curl = new Curl(supportHttp2);
        }

        public CurlHttpClient(ICoroutineRunner runner) : this(runner, false)
        {
        }

        override public void Dispose()
        {
            base.Dispose();
            DisconnectAppEvents();
            _curl.Dispose();
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