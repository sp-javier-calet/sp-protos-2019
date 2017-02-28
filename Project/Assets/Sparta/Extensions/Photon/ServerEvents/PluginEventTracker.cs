using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Photon.ServerEvents
{
    public class PluginEventTracker : IUpdateable
    {
        //TODO: specify
        const string MetricUri = "/api/v3/rtmp/metrics";
        const string TrackUri = "/api/v3/rtmp/tracks";
        const string LogUri = "/api/v3/rtmp/logs";

        public const int DefaultSendInterval = 10;

        public int SendInterval = DefaultSendInterval;

        public delegate void SetupHttpRequestDelegate(HttpRequest req,string uri);

        public SetupHttpRequestDelegate SetupRequest;

        IHttpClient _httpClient;
        Dictionary<MetricType, List<Metric>> _pendingMetrics;
        List<Event> _pendingEvents;
        IUpdateScheduler _updateScheduler;

        public PluginEventTracker(IUpdateScheduler updateScheduler, IHttpClient httpClient)
        {
            _updateScheduler = updateScheduler;
            _httpClient = httpClient;//new ImmediateWebRequestHttpClient();
            _pendingMetrics = new Dictionary<MetricType, List<Metric>>();
            _pendingEvents = new List<Event>();
        }

        public void Start()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this, false, SendInterval);
            }
        }

        #region IUpdateable implementation

        public void Update()
        {
            DoSendMetrics();
            DoSendTracks();
        }

        #endregion

        public void SendMetric(Metric metric, ErrorDelegate del = null)
        {
            if(!_pendingMetrics.ContainsKey(metric.MetricType))
            {
                _pendingMetrics.Add(metric.MetricType, new List<Metric>());
            }
            _pendingMetrics[metric.MetricType].Add(metric);
        }

        void DoSendMetrics()
        {
            if(_pendingMetrics.Count == 0)
            {
                return;
            }
            var req = new HttpRequest();
            if(SetupRequest != null)
            {
                SetupRequest(req, MetricUri);
            }

            var metricsData = new AttrDic();
            var sendMetrics = new List<Metric>();
            foreach(var key in _pendingMetrics.Keys)
            {
                var metrics = new AttrList();
                for(int i = 0; i < _pendingMetrics[key].Count; i++)
                {
                    var metric = _pendingMetrics[key][i];
                    metrics.Add(metric.ToAttr());
                    sendMetrics.Add(metric);
                }
                var dicKey = key.ToString().ToLower() + "s";
                metricsData.Set(dicKey, metrics);
            }
            req.Body = new JsonAttrSerializer().Serialize(metricsData);
            _httpClient.Send(req, (r) => OnMetricResponse(r, sendMetrics));
        }

        void OnMetricResponse(HttpResponse resp, List<Metric> sendMetrics)
        {
            if(!resp.HasError)
            {
                for(int i = 0; i < sendMetrics.Count; i++)
                {
                    var metric = sendMetrics[i];
                    _pendingMetrics[metric.MetricType].Remove(metric);
                    //TODO call metric on response delegate?
                }
            }
        }

        public void SendTrack(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            var ev = new Event(eventName, data?? new AttrDic(), del);
            _pendingEvents.Add(ev);
        }

        public void SendTrack(Event ev)
        {
            _pendingEvents.Add(ev);
        }

        void DoSendTracks()
        {
            if(_pendingEvents.Count == 0)
            {
                return;
            }
            var req = new HttpRequest();
            if(SetupRequest != null)
            {
                SetupRequest(req, TrackUri);
            }
            var track = new AttrDic();
            var events = new List<Event>(_pendingEvents);
            var eventsAttr = new AttrList();
            for(int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                eventsAttr.Add(ev.ToAttr());
            }
            var common = new AttrDic();
            common.Set("plat", new AttrString("photon"));
            common.Set("ver", new AttrString("1"));
            track.Set("common",common);
            track.Set("events", eventsAttr);
            req.Body = new JsonAttrSerializer().Serialize(track);
            _httpClient.Send(req, r => OnSendEventResponse(r, events));
        }

        void OnSendEventResponse(HttpResponse resp, List<Event> sendEvents)
        {
            if(!resp.HasError)
            {
                for(int i = 0; i < sendEvents.Count; i++)
                {
                    var ev = sendEvents[i];
                    if(ev.ResponseDelegate != null)
                    {
                        ev.ResponseDelegate(resp.Error);
                    }
                    _pendingEvents.Remove(ev);
                }
            }
        }

        public void SendLog(Log log, ErrorDelegate del = null)
        {
            var req = new HttpRequest();
            if (SetupRequest != null)
            {
                SetupRequest(req, LogUri);
            }
            req.Body = new JsonAttrSerializer().Serialize(log.ToAttr());
            _httpClient.Send(req, r => OnSendLogResponse(r, del));
        }

        void OnSendLogResponse(HttpResponse resp, ErrorDelegate del)
        {
            if(del != null)
            {
                del(resp.Error);
            }
        }

    }
}