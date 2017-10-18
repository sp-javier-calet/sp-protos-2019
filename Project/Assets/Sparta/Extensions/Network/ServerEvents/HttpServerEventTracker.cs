using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Network.ServerEvents
{
    public class HttpServerEventTracker : IUpdateable
    {
        // datadog
        const string MetricUri = "rtmp/metrics";
        // analytics - backoffice
        const string TrackUri = "rtmp/tracks";
        // kibana
        const string LogUri = "rtmp/logs";

        public Func<string> GetBaseUrlCallback;
        public string Environment;
        public string Platform;

        public const int DefaultSendInterval = 10;

        public int SendInterval = DefaultSendInterval;

        public event Action<AttrDic> UpdateCommonTrackData;

        IUpdateScheduler _updateScheduler;
        IHttpClient _httpClient;
        Dictionary<MetricType, List<Metric>> _pendingMetrics;
        Dictionary<MetricType, List<Metric>> _sendingMetrics;
        List<Event> _pendingEvents;
        List<Event> _sendingEvents;
        List<Log> _pendingLogs;
        List<Log> _sendingLogs;
        bool _sending;
        bool _sendAgain;

        public HttpServerEventTracker(IUpdateScheduler updateScheduler, IHttpClient httpClient)
        {
            _updateScheduler = updateScheduler;
            _httpClient = httpClient;
            _pendingMetrics = new Dictionary<MetricType, List<Metric>>();
            _sendingMetrics = new Dictionary<MetricType, List<Metric>>();
            _pendingEvents = new List<Event>();
            _sendingEvents = new List<Event>();
            _pendingLogs = new List<Log>();
            _sendingLogs = new List<Log>();
        }

        public void Start()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, SendInterval);
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

        public bool HasPendingData
        {
            get
            {
                return _pendingMetrics.Count > 0 || _sendingMetrics.Count > 0 ||
                _pendingEvents.Count > 0 || _sendingEvents.Count > 0 ||
                _pendingLogs.Count > 0 || _sendingLogs.Count > 0;
            }
        }

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
            var itr = _pendingMetrics.GetEnumerator();
            while(itr.MoveNext())
            {
                var metrics = new AttrList();
                var metricList = itr.Current.Value;
                for(int i = 0; i < metricList.Count; ++i)
                {
                    var metric = metricList[i];
                    if(!string.IsNullOrEmpty(Environment))
                    {
                        metric.Tags.Add(string.Format("environment:{0}", Environment));
                    }
                    metrics.Add(metric.ToAttr());
                }

                var metricType = itr.Current.Key;
                metricsData.Set(metricType.ToApiKey(), metrics);
                if(!_sendingMetrics.ContainsKey(metricType))
                {
                    _sendingMetrics.Add(metricType, new List<Metric>());
                }
                _sendingMetrics[metricType].AddRange(metricList);
            }
            itr.Dispose();
            _pendingMetrics.Clear();
            req.Body = new JsonAttrSerializer().Serialize(metricsData);
            _httpClient.Send(req, OnMetricResponse);
        }

        void OnMetricResponse(HttpResponse resp)
        {
            if(!resp.HasError)
            {
                var itr = _sendingMetrics.GetEnumerator();
                while(itr.MoveNext())
                {
                    var metricList = itr.Current.Value;
                    for(int i = 0; i < metricList.Count; ++i)
                    {
                        var metric = metricList[i];
                        if(metric.ResponseDelegate != null)
                        {
                            metric.ResponseDelegate(resp.Error);
                        }
                    }
                }
                itr.Dispose();
            }
            else
            {
                var itr = _sendingMetrics.GetEnumerator();
                while(itr.MoveNext())
                {
                    var metricType = itr.Current.Key;
                    var metricList = itr.Current.Value;

                    if(!_pendingMetrics.ContainsKey(metricType))
                    {
                        _pendingMetrics.Add(metricType, new List<Metric>());
                    }

                    _pendingMetrics[metricType].AddRange(metricList);
                }
                itr.Dispose();
            }

            _sendingMetrics.Clear();
        }

        public void SendTrack(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            var ev = new Event(eventName, data ?? new AttrDic(), del);
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
            _sendingEvents.AddRange(_pendingEvents);
            var eventsAttr = new AttrList();
            for(int i = 0; i < _pendingEvents.Count; ++i)
            {
                var ev = _pendingEvents[i];
                eventsAttr.Add(ev.ToAttr());
            }
            _pendingEvents.Clear();

            var common = new AttrDic();
            var handler = UpdateCommonTrackData;
            if(handler != null)
            {
                UpdateCommonTrackData(common);
            }
            common.SetValue("plat", Platform);
            track.Set("common", common);
            track.Set("events", eventsAttr);
            req.Body = new JsonAttrSerializer().Serialize(track);
            _httpClient.Send(req, OnSendEventResponse);
        }

        void OnSendEventResponse(HttpResponse resp)
        {
            if(!resp.HasError)
            {
                for(int i = 0; i < _sendingEvents.Count; ++i)
                {
                    var ev = _sendingEvents[i];
                    if(ev.ResponseDelegate != null)
                    {
                        ev.ResponseDelegate(resp.Error);
                    }
                }
            }
            else
            {
                _pendingEvents.AddRange(_sendingEvents);
            }

            _sendingEvents.Clear();
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
            if(_sending || _pendingLogs.Count == 0)
            {
                return;
            }
            var req = new HttpRequest();
            SetupRequest(req, LogUri);
            var body = new AttrDic();
            _sendingLogs.AddRange(_pendingLogs);
            var logList = new AttrList();
            for(int i = 0; i < _pendingLogs.Count; ++i)
            {
                var log = _pendingLogs[i];
                if(!string.IsNullOrEmpty(Environment))
                {
                    log.Context.SetValue("environment", Environment);
                }
                logList.Add(log.ToAttr());
            }
            _pendingLogs.Clear();
            body.Set("logs", logList);
            req.Body = new JsonAttrSerializer().Serialize(body);
            _sending = true;
            _httpClient.Send(req, r => OnSendLogResponse(r));
        }

        void OnSendLogResponse(HttpResponse resp)
        {
            if(!resp.HasError)
            {
                for(int i = 0; i < _sendingLogs.Count; ++i)
                {
                    var log = _sendingLogs[i];
                    if(log.ResponseDelegate != null)
                    {
                        log.ResponseDelegate(resp.Error);
                    }
                }
            }
            else
            {
                _pendingLogs.AddRange(_sendingLogs);
            }

            _sendingLogs.Clear();
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
            Uri.TryCreate(StringUtils.CombineUri(GetBaseUrlCallback(), uri), UriKind.Absolute, out auxUri);
            req.Url = auxUri;
        }
    }
}