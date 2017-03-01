using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Network.ServerEvents
{
    public class PluginEventTracker : IUpdateable
    {
        //TODO: specify
        const string MetricUri = "rtmp/metrics";
        const string TrackUri = "rtmp/tracks";
        const string LogUri = "rtmp/logs";

        public string BaseUrl;

        public const int DefaultSendInterval = 10;

        public int SendInterval = DefaultSendInterval;

        IHttpClient _httpClient;
        Dictionary<MetricType, List<Metric>> _pendingMetrics;
        List<Event> _pendingEvents;
        IUpdateScheduler _updateScheduler;
        List<Log> _pendingLogs;
        List<Log> _sendingLogs;
        bool _sending;
        bool _sendAgain;

        public PluginEventTracker(IUpdateScheduler updateScheduler, IHttpClient httpClient)
        {
            _updateScheduler = updateScheduler;
            _httpClient = httpClient;//new ImmediateWebRequestHttpClient();
            _pendingMetrics = new Dictionary<MetricType, List<Metric>>();
            _pendingEvents = new List<Event>();
            _pendingLogs = new List<Log>();
            _sendingLogs = new List<Log>();
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
            DoSendLogs();
        }

        #endregion

        public void SendMetric(Metric metric)
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
            SetupRequest(req, MetricUri);
            
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
                    if(metric.ResponseDelegate != null)
                    {
                        metric.ResponseDelegate(resp.Error);
                    }
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
            SetupRequest(req, TrackUri);
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

        public void SendLog(Log log, bool immediate = false)
        {
            _pendingLogs.Add(log);
            if(immediate)
            {
                if(_sending)
                {
                    _sendAgain = true;
                }
                else
                {
                    DoSendLogs();
                }
            }
        }

        void DoSendLogs()
        {
            if(_sending)
            {
                return;
            }
            var req = new HttpRequest();
            SetupRequest(req, LogUri);
            var body = new AttrDic();
            var logList = new AttrList();
            for (int i = 0; i < _pendingLogs.Count; i++)
            {
                var log = _pendingLogs[i];
                logList.Add(log.ToAttr());
                _sendingLogs.Add(log);
            }
            _pendingLogs.Clear();
            body.Set("logs", logList);
            req.Body = new JsonAttrSerializer().Serialize(body);
            _sending = true;
            _httpClient.Send(req, r => OnSendLogResponse(r));
        }

        void OnSendLogResponse(HttpResponse resp)
        {
            for (int i = 0; i < _sendingLogs.Count; i++)
            {
                var log = _sendingLogs[i];
                if (log.ResponseDelegate != null)
                {
                    log.ResponseDelegate(resp.Error);
                }
            }
            _sending = false;
            if(_sendAgain)
            {
                _sendAgain = false;
                DoSendLogs();
            }
        }

        void SetupRequest(HttpRequest req, string uri)
        {
            req.Method = HttpRequest.MethodType.POST;
            Uri auxUri;
            Uri.TryCreate(StringUtils.CombineUri(BaseUrl, uri), UriKind.Absolute, out auxUri);
            req.Url = auxUri;
        }
    }
}