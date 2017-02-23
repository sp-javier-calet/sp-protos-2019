using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    class PluginEventTracker : IUpdateable
    {
        //TODO: specify
        const string MetricUri = "/api/v3/rtmp/metrics";
        const string TrackUri = "/api/v3/rtmp/tracks";

        public const int DefaultSendInterval = 10;

        public int SendInterval = DefaultSendInterval;

        public delegate void SetupHttpRequestDelegate(HttpRequest req,string uri);

        public SetupHttpRequestDelegate SetupRequest;

        IHttpClient _httpClient;
        Dictionary<MetricType, List<Metric>> _pendingMetrics;
        List<Event> _pendingEvents;
        UpdateScheduler _updateScheduler;

        public PluginEventTracker(UpdateScheduler updateScheduler)
        {
            _updateScheduler = updateScheduler;
            _httpClient = new ImmediateWebRequestHttpClient();
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
                metricsData.Set(key.ToString(), metrics);
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

        //TODO: ask if there is some server_id required for this tracks
        public void SendTrack(string eventName, AttrDic data = null)
        {
            var ev = new Event(eventName, data);
            _pendingEvents.Add(ev);
        }

        public void SendTrack(Event ev)
        {
            _pendingEvents.Add(ev);
        }

        void DoSendTracks()
        {
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
                    _pendingEvents.Remove(ev);
                }
            }
        }

    }
}