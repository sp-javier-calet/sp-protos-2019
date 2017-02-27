using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using System;
using SocialPoint.ServerEvents;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Events")]
    public class PluginEventTrackerTests
    {
        PluginEventTracker EventTracker;
        IUpdateScheduler Scheduler;
        IHttpClient HttpClient;

        [SetUp]
        public void Setup()
        {
            Scheduler = Substitute.For<IUpdateScheduler>();
            HttpClient = Substitute.For<IHttpClient>();
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));
            EventTracker = new PluginEventTracker(Scheduler, HttpClient);
            EventTracker.SetupRequest = SetupHttpRequest;
            EventTracker.Start();
        }

        void SetupHttpRequest(HttpRequest req, string uri)
        {
            Uri url;
            Uri.TryCreate("http://example.com" + uri, UriKind.Absolute, out url);
            req.Url = url;
        }

        [Test]
        public void Start()
        {
            Scheduler.Received(1).Add(EventTracker, false, EventTracker.SendInterval);
        }


        [Test]
        public void Update()
        {
            EventTracker.Update();
        }

        [Test]
        public void UpdateCallsSend()
        {
            EventTracker.Update();
            HttpClient.Received(2).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void RequestIsSetUp()
        {
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => r.Url.AbsoluteUri == "http://example.com/api/v3/rtmp/metrics"), Arg.Any<HttpResponseDelegate>());
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => r.Url.AbsoluteUri == "http://example.com/api/v3/rtmp/tracks"), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void MetricIsSend()
        {
            var metric = new Metric(MetricType.Counter, "Tests", 1);
            EventTracker.SendMetric(metric);
            EventTracker.Update();
            Predicate<HttpRequest> pred = delegate (HttpRequest req)
            {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                return data.ContainsKey(MetricType.Counter.ToString());
            };
            HttpClient.Received().Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void EventIsSend()
        {
            EventTracker.SendTrack("Test");
            EventTracker.Update();
            Predicate<HttpRequest> pred = delegate (HttpRequest req)
            {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if (data.ContainsKey("events"))
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
            Predicate<HttpRequest> pred = delegate (HttpRequest req)
            {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if(data.ContainsKey(MetricType.Counter.ToString()))
                {
                    return data[MetricType.Counter.ToString()].AsList.Count > 0;
                }
                return false;
            };

            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
            EventTracker.Update();
            HttpClient.Received(1).Send(Arg.Is<HttpRequest>(r => pred(r)), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void EventIsDeletedAfterSend()
        {

            EventTracker.SendTrack("Test");
            Predicate<HttpRequest> pred = delegate (HttpRequest req)
            {
                var data = new JsonAttrParser().Parse(req.Body).AsDic;
                if (data.ContainsKey("events"))
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
    }
}