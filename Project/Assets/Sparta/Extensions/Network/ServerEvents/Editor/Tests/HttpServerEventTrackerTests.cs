using System;
using NSubstitute;
using NUnit.Framework;
using SocialPoint.Attributes;
using SocialPoint.Network.ServerEvents;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Events")]
    public class HttpServerEventTrackerTests
    {
        HttpServerEventTracker EventTracker;
        IUpdateScheduler Scheduler;
        IHttpClient HttpClient;

        [SetUp]
        public void Setup()
        {
            Scheduler = Substitute.For<IUpdateScheduler>();
            HttpClient = Substitute.For<IHttpClient>();
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));
            EventTracker = new HttpServerEventTracker(Scheduler, HttpClient);
            EventTracker.GetBaseUrlCallback = () => "https://lodx.socialpointgames.com/api/v3/";
            EventTracker.Start();
        }

        [Test]
        public void Start()
        {
            Scheduler.Received(1).Add(EventTracker, UpdateableTimeMode.GameTimeUnscaled, EventTracker.SendInterval);
        }


        [Test]
        public void Update()
        {
            EventTracker.Update();
        }

        [Test]
        public void UpdateCallsSend()
        {
            EventTracker.SendTrack("Test");
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            EventTracker.Update();
            HttpClient.Received(2).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void RequestIsSetUp()
        {
            EventTracker.SendTrack("Test");
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => r.Url.AbsoluteUri == "https://lodx.socialpointgames.com/api/v3/rtmp/metrics"), Arg.Any<HttpResponseDelegate>());
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => r.Url.AbsoluteUri == "https://lodx.socialpointgames.com/api/v3/rtmp/tracks"), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void MetricIsSend()
        {
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            EventTracker.Update();
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                return data.ContainsKey(MetricType.Counter.ToApiKey());
            };
            HttpClient.Received().Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void EventIsSend()
        {
            EventTracker.SendTrack("Test");
            EventTracker.Update();
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if(data.ContainsKey("events"))
                {
                    var events = data["events"].AsList;
                    var ev = events[0].AsDic;
                    return ev["type"].AsValue == "Test";
                }
                return false;
            };
            HttpClient.Received().Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void MetricIsDeletedAfterSend()
        {
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if(data.ContainsKey(MetricType.Counter.ToApiKey()))
                {
                    return data[MetricType.Counter.ToApiKey()].AsList.Count > 0;
                }
                return false;
            };

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void MultipleMetricsAtATime([Random(1, 3, 2)] int counter, [Random(1, 3, 2)] int gauge)
        {
            var counterCount = counter;
            var gaugesCount = gauge;
            var random = new Random();
            while(counterCount + gaugesCount != 0)
            {
                var r = random.NextDouble();
                if(r <= 0.5f)
                {
                    if(counterCount > 0)
                    {
                        EventTracker.SendMetric(new Metric(MetricType.Counter, "Counter", 1));
                        counterCount--;
                    }
                    else if(gaugesCount > 0)
                    {
                        EventTracker.SendMetric(new Metric(MetricType.Gauge, "Gauge", 1));
                        gaugesCount--;
                    }
                }
                else
                {
                    if(gaugesCount > 0)
                    {
                        EventTracker.SendMetric(new Metric(MetricType.Gauge, "Gauge", 1));
                        gaugesCount--;
                    }
                    else if(counterCount > 0)
                    {
                        EventTracker.SendMetric(new Metric(MetricType.Counter, "Counter", 1));
                        counterCount--;
                    }
                }
            }
            EventTracker.Update();
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                bool failed = !data.ContainsKey(MetricType.Counter.ToApiKey());
                var counterMetrics = data[MetricType.Counter.ToApiKey()].AsList;
                foreach(var metric in counterMetrics)
                {
                    var dic = metric.AsDic;
                    failed |= dic["stat"].ToString().Equals("Gauge");
                }
                failed |= data[MetricType.Counter.ToApiKey()].AsList.Count != counter;
                failed |= !data.ContainsKey(MetricType.Gauge.ToApiKey());
                failed |= data[MetricType.Gauge.ToApiKey()].AsList.Count != gauge;
                var gaugeMetrics = data[MetricType.Gauge.ToApiKey()].AsList;
                foreach(var metric in gaugeMetrics)
                {
                    var dic = metric.AsDic;
                    failed |= dic["stat"].ToString().Equals("Counter");
                }
                return !failed;
            };
            HttpClient.Received().Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void EventIsDeletedAfterSend()
        {
            EventTracker.SendTrack("Test");
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if(data.ContainsKey("events"))
                {
                    var events = data["events"].AsList;
                    var ev = events[0].AsDic;
                    return ev["type"].AsValue == "Test";
                }
                return false;
            };
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void SendLog_Immediate()
        {
            EventTracker.SendLog(new Log(LogLevel.Error, "TestMessage"), true);
            HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void SendLog()
        {
            EventTracker.SendLog(new Log(LogLevel.Error, "TestMessage"));
            HttpClient.Received(0).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void SendMetricWithMultipleUpdates()
        {
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            EventTracker.Update();
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void SendTrakWithMultipleUpdates()
        {
            EventTracker.SendTrack("test-track");
            EventTracker.Update();
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void SendLogWithMultipleUpdates()
        {
            EventTracker.SendLog(new Log(LogLevel.Error, "TestMessage"), true);
            EventTracker.Update();
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void MetricIsNotDeletedAfterSendError()
        {
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                return data.ContainsKey("counters");
            };
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse((int)HttpResponse.StatusCodeType.NotAvailableError)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());

            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            HttpClient.ClearReceivedCalls();

            EventTracker.Update();
            HttpClient.DidNotReceiveWithAnyArgs().Send(null, null);
        }

        [Test]
        public void TrackIsNotDeletedAfterSendError()
        {
            EventTracker.SendTrack("Test");
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                return data.ContainsKey("events");
            };
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse((int)HttpResponse.StatusCodeType.NotAvailableError)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());

            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            HttpClient.ClearReceivedCalls();

            EventTracker.Update();
            HttpClient.DidNotReceiveWithAnyArgs().Send(null, null);
        }

        [Test]
        public void LogIsNotDeletedAfterSendError()
        {
            EventTracker.SendLog(new Log(LogLevel.Error, "TestMessage"), true);
            Predicate<HttpRequest> pred = delegate (HttpRequest req) {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                return data.ContainsKey("logs");
            };
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse((int)HttpResponse.StatusCodeType.NotAvailableError)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());

            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            HttpClient.ClearReceivedCalls();

            EventTracker.Update();
            HttpClient.DidNotReceiveWithAnyArgs().Send(null, null);
        }
    }
}