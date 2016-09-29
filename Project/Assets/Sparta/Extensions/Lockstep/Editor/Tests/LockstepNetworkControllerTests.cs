using NUnit.Framework;
using NSubstitute;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    class TestCommand : ILockstepCommand
    {
        public int Value{ get; private set; }

        public TestCommand(int v=0)
        {
            Value = v;
        }

        public object Clone()
        {
            return new TestCommand(Value);
        }

        public void Deserialize(IReader reader)
        {
            Value = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Value);
        }
    }

    class TestCommandLogic : ILockstepCommandLogic<TestCommand>
    {
        public int SumValues{ get; private set; }

        public void Apply(TestCommand data)
        {
            SumValues += data.Value;
        }
    }


    [TestFixture]
    [Category("SocialPoint.Lockstep")]
    class LockstepNetworkControllerTests
    {
        LocalNetworkServer _netServer;
        ServerLockstepNetworkController _netLockServer;

        LocalNetworkClient _netClient1;
        ClientLockstepController _lockClient1;
        ClientLockstepNetworkController _netLockClient1;

        LocalNetworkClient _netClient2;
        ClientLockstepController _lockClient2;
        ClientLockstepNetworkController _netLockClient2;

        UpdateScheduler _scheduler;
        LockstepCommandFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _netServer = new LocalNetworkServer();
            _scheduler = new UpdateScheduler();
            _factory = new LockstepCommandFactory();

            _netLockServer = new ServerLockstepNetworkController(_netServer, _scheduler);
            _netLockServer.ServerConfig.MaxPlayers = 2;

            _netClient1 = new LocalNetworkClient(_netServer);
            _lockClient1 = new ClientLockstepController();
            _netLockClient1 = new ClientLockstepNetworkController(_netClient1, _lockClient1, _factory);

            _netClient2 = new LocalNetworkClient(_netServer);
            _lockClient2 = new ClientLockstepController();
            _netLockClient2 = new ClientLockstepNetworkController(_netClient2, _lockClient2, _factory);
        }

        void Update(int dt)
        {
            _netLockServer.Update(dt);
            _lockClient1.Update(dt);
            _lockClient2.Update(dt);
        }

        [Test]
        public void StartupWorking()
        {
            _netLockServer.Config.CommandStepDuration = 399;

            _netServer.Start();
            _netClient1.Connect();

            Assert.AreEqual(1, _netLockServer.PlayerCount, "When a client connects, the server creates a player");
            Assert.AreEqual(0, _netLockServer.ReadyPlayerCount, "When a client connects, player is not ready");

            _netLockClient1.SendPlayerReady();

            Assert.AreEqual(1, _netLockServer.ReadyPlayerCount, "Server gets player ready");
            Assert.IsFalse(_netLockServer.Running, "Server waits for max players to start");

            _netLockClient1.SendPlayerReady();

            Assert.AreEqual(1, _netLockServer.ReadyPlayerCount, "Same client does not count as multiple players");

            _netLockClient2.SendPlayerReady();

            Assert.AreEqual(1, _netLockServer.ReadyPlayerCount, "Client does not send ready if not connected");

            _netClient2.Connect();

            Assert.AreEqual(2, _netLockServer.ReadyPlayerCount, "Client sends ready after connected");

            Assert.IsTrue(_netLockServer.Running, "Server waits for max players to start");

            Assert.AreEqual(_netLockServer.Config.CommandStepDuration, _lockClient1.Config.CommandStepDuration, "Lockstep config is sent to the clients");
        }

        [Test]
        public void SendingCommands()
        {
            _netLockServer.ServerConfig.ClientSimulationDelay = 50;
            _netLockServer.ServerConfig.ClientStartDelay = 100;
            _factory.Register<TestCommand>(1, new TestCommand());
            var logic1 = new TestCommandLogic();
            _lockClient1.RegisterCommandLogic<TestCommand>(logic1);

            _netServer.Start();
            _netClient1.Connect();
            _netClient2.Connect();
            _netLockClient1.SendPlayerReady();
            _netLockClient2.SendPlayerReady();

            Update(100);

            _lockClient2.AddPendingCommand(new TestCommand(4));

            Assert.AreEqual(0, logic1.SumValues, "Clients will not get commands before start plus sim delay");

            Update(150);

            Assert.AreEqual(4, logic1.SumValues, "Commands are sent from the clients to the server and back");
        }
    }

}