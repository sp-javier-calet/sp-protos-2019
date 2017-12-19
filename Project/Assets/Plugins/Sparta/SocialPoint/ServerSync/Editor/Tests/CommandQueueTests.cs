using System;
using NSubstitute;
using NUnit.Framework;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Login;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.ServerSync
{

    [TestFixture]
    [Category("SocialPoint.ServerSync")]
    class CommandQueueTests
    {

        CommandQueue CommandQueue;
        IHttpClient HttpClient;
        //needed for the monobehavior
        GameObject GO;

        [SetUp]
        public void SetUp()
        {
            GO = new GameObject();
            HttpClient = Substitute.For<IHttpClient>();
            var runner = GO.AddComponent<UnityUpdateRunner>();
            CommandQueue = new CommandQueue(runner, HttpClient);

            CommandQueue.LoginData = Substitute.For<ILoginData>();
            CommandQueue.LoginData.When(x => x.SetupHttpRequest(Arg.Any<HttpRequest>(), Arg.Any<string>()))
                .Do(x => { 
                    var req = x.Arg<HttpRequest>();
                    var url = x.Arg<string>();
                    req .Method = HttpRequest.MethodType.POST;
                    req.Url = new Uri("http://" + url);
                    req.AddQueryParam("session_id", new AttrString("session_id_test"));
                });

            var appEvents = Substitute.For<IAppEvents>();
            appEvents.WillGoBackground.Returns(new PriorityAction());
            appEvents.WasOnBackground.Returns(new PriorityAction());
            appEvents.GameWasLoaded.Returns(new PriorityAction());
            appEvents.GameWillRestart.Returns(new PriorityAction());
            CommandQueue.AppEvents = appEvents;
            CommandQueue.TrackSystemEvent = Substitute.For<CommandQueue.TrackEventDelegate>();
        }

        [Test]
        public void Start()
        {
            CommandQueue.Start();        
        }

        [Test]
        public void Stop()
        {
            Start();
            CommandQueue.Stop();
        }

        [Test]
        public void Reset()
        {
            Start();
            CommandQueue.Reset();
        }

        [Test]
        public void Add_cmd_finishDelegate()
        {
            Start();
            CommandQueue.Add(Substitute.For<Command>("Test Command", null, false, true), Substitute.For<Action<Attr,Error>>());
            //todo: check number of packages?
        }

        [Test]
        public void Remove_removes_TestCommand_added()
        {
            Start();
            Add_cmd_finishDelegate();
            Assert.AreEqual(1, CommandQueue.Remove(item => item.Command.Name == "Test Command"));
        }

        [Test]
        public void Flush_action()
        {
            Start();
            CommandQueue.Flush(Substitute.For<Action>());
            //TODO: check number of packets or something
        }

        [Test]
        public void Flush_finishDelegate()
        {
            Start();
            CommandQueue.Flush();
            //TODO: check number of packets or something
        }

        [Test]
        public void Send_calls_HttpClient_Send()
        {
            Start();
            Add_cmd_finishDelegate();
            Flush_action();
            CommandQueue.Send(Substitute.For<Action>());
            //CommandQueue.RequestSetup.Received(1).Invoke(Arg.Any<HttpRequest>(), Arg.Any<string>());
            HttpClient.ReceivedWithAnyArgs(1).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void Multiple_Send_calls_HttpClient_Send()
        {
            Start();

            // Call Response callback to finalize the request immediately.
            HttpClient.Send(Arg.Any<HttpRequest>(), Arg.InvokeDelegate<HttpResponseDelegate>(new HttpResponse(200)));

            Add_cmd_finishDelegate();
            Flush_action();
            CommandQueue.Send(null);

            Add_cmd_finishDelegate();
            Flush_action();
            CommandQueue.Send(null);

            HttpClient.ReceivedWithAnyArgs(2).Send(Arg.Any<HttpRequest>(), Arg.Any<HttpResponseDelegate>());
        }

        [Test]
        public void Dispose()
        {
            Start();
            CommandQueue.Dispose();
        }


        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(GO);
        }


        /*
        void Start();
        void Uninitialize();
        void Stop();
        void Reset();
        Petition Add(Command cmd, Action callback);
        Petition Add(Command cmd, PackedCommand.FinishDelegate callback=null);
        int Remove(Packet.FilterDelegate callback = null);
        void Flush(Action callback);
        void Flush(Packet.FinishDelegate callback = null);
        bool Send(Action finish = null);
        */
    }
}
