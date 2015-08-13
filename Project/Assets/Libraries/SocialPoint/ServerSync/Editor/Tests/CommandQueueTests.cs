using NUnit.Framework;
using NSubstitute;

using System;
using UnityEngine;

using SocialPoint.Network;
using SocialPoint.AppEvents;

namespace SocialPoint.ServerSync
{

    [TestFixture]
    [Category("SocialPoint.ServerSync")]
    internal class CommandQueueTests
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
            var monobh = GO.AddComponent<MonoBehaviour>();
            CommandQueue = new CommandQueue(monobh, HttpClient);

            //CommandQueue.RequestSetup = Substitute.For<CommandQueue.RequestSetupDelegate>();
            CommandQueue.RequestSetup = (req, Uri) => {
                req.Method = HttpRequest.MethodType.POST;
                req.Url = new Uri("http://"+Uri);
                req.AddQueryParam("session_id",new SocialPoint.Attributes.AttrString("session_id_test"));
            };
            CommandQueue.AppEvents = Substitute.For<IAppEvents>();
            CommandQueue.TrackEvent = Substitute.For<CommandQueue.TrackEventDelegate>();
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
        public void Add_cmd_action()
        {
            Start();
            CommandQueue.Add(Substitute.For<Command>("Test Command",null,false,true), Substitute.For<Action>());
            //todo: check number of packages?
        }

        [Test]
        public void Add_cmd_finishDelegate()
        {
            Start();
            CommandQueue.Add(Substitute.For<Command>("Test Command",null,false,true), Substitute.For<PackedCommand.FinishDelegate>());
            //todo: check number of packages?
        }

        [Test]
        public void Remove_removes_TestCommand_added()
        {
            Start();
            Add_cmd_action();
            Assert.AreEqual(1,CommandQueue.Remove((PackedCommand item) => item.Command.Name == "Test Command"));
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
            Add_cmd_action();
            Flush_action();
            CommandQueue.Send(Substitute.For<Action>());
            //CommandQueue.RequestSetup.Received(1).Invoke(Arg.Any<HttpRequest>(), Arg.Any<string>());
            HttpClient.ReceivedWithAnyArgs(1).Send(Arg.Any<HttpRequest>(),Arg.Any<HttpResponseDelegate>());
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
