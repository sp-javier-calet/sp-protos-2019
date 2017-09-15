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

        public void Apply(TestCommand data, byte playerNum)
        {
            SumValues += data.Value;
        }
    }

    [TestFixture]
    [Category("SocialPoint.Lockstep")]
    class LockstepNetworkTests
    {
        LocalNetworkServer _netServer;
        LockstepNetworkServer _netLockServer;

        SimulateNetworkClient _netClient1;
        LockstepClient _lockClient1;
        LockstepNetworkClient _netLockClient1;
        TestCommandLogic _logic1;

        SimulateNetworkClient _netClient2;
        LockstepClient _lockClient2;
        LockstepNetworkClient _netLockClient2;

        LockstepCommandFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _netServer = new LocalNetworkServer();
            _factory = new LockstepCommandFactory();

            _netLockServer = new LockstepNetworkServer(_netServer);
            _netLockServer.ServerConfig.MaxPlayers = 2;

            _netClient1 = new SimulateNetworkClient(_netServer);
            _lockClient1 = new LockstepClient();
            _netLockClient1 = new LockstepNetworkClient(_netClient1, _lockClient1, _factory);

            _netClient2 = new SimulateNetworkClient(_netServer);
            _lockClient2 = new LockstepClient();
            _netLockClient2 = new LockstepNetworkClient(_netClient2, _lockClient2, _factory);

            _factory.Register<TestCommand>(1, new TestCommand());
            _logic1 = new TestCommandLogic();
            _lockClient1.RegisterCommandLogic<TestCommand>(_logic1);
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

        void StartMatch()
        {
            _netLockServer.ServerConfig.ClientSimulationDelay = 200;
            _netLockServer.ServerConfig.ClientStartDelay = 500;
            _netLockServer.Config.CommandStepDuration = 100;
            _netLockServer.Config.SimulationStepDuration = 10;
            _lockClient1.ClientConfig.MaxSimulationStepsPerFrame = 10;

            _netServer.Start();
            _netClient1.Connect();
            _netClient2.Connect();
            _netLockClient1.SendPlayerReady();
            _netLockClient2.SendPlayerReady();
        }

        [Test]
        public void SendingCommands()
        {
            StartMatch();

            Update(100);

            _lockClient2.AddPendingCommand(new TestCommand(4));

            Assert.AreEqual(0, _logic1.SumValues, "Clients will not get commands before start");

            Update(100);
            Update(100);
            Update(100);
            Update(100);

            _lockClient2.AddPendingCommand(new TestCommand(5));

            Update(100);
            Update(100);
            Update(50);

            Assert.AreEqual(0, _logic1.SumValues, "Client did not get command because of the client sim delay");

            Update(50);

            Assert.AreEqual(5, _logic1.SumValues, "Commands are sent from the clients to the server and back");
        }


        [Test]
        public void LagSituation()
        {
            StartMatch();

            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(50);

            Assert.IsTrue(_lockClient1.Connected, "Client is connected");

            _netClient1.BlockReception = true;

            Update(100);
            Update(100);
            Update(100);

            _lockClient2.AddPendingCommand(new TestCommand(4));

            Update(100);
            Update(100);

            Assert.IsFalse(_lockClient1.Connected, "Client is lagging, it's not getting any messages");
            Assert.AreEqual(0, _logic1.SumValues, "Client is lagging, it's not getting any messages");

            _netClient1.BlockReception = false;

            Update(100);
            Update(50);

            Assert.AreEqual(0, _logic1.SumValues, "Catches up at MaxSimulationStepsPerFrame");

            Update(100);

            Assert.AreEqual(4, _logic1.SumValues, "Catches up at MaxSimulationStepsPerFrame");
        }

        [Test]
        public void ReconnectSamePlayerId()
        {
            _netLockServer.ServerConfig.MaxPlayers = 3;

            var localClient = new LockstepClient();
            _netLockServer.RegisterLocalClient(localClient, _factory);

            StartMatch();

            _netLockServer.LocalPlayerReady();

            var playerNum1 = _netLockClient1.PlayerNumber;
            var playerNum2 = _netLockClient2.PlayerNumber;
            var playerNum3 = _netLockServer.LocalPlayerNumber;

            Assert.AreEqual(0, playerNum1, "Player nums should be consecutive");
            Assert.AreEqual(1, playerNum2, "Player nums should be consecutive");
            Assert.AreEqual(2, playerNum3, "Player nums should be consecutive");

            _netClient1.Disconnect();
            _netClient2.Disconnect();
            _netLockServer.UnregisterLocalClient();

            _netClient2.Connect();
            _netLockClient2.SendPlayerReady();

            _netLockServer.RegisterLocalClient(localClient, _factory);
            _netLockServer.LocalPlayerReady();

            _netClient1.Connect();
            _netLockClient1.SendPlayerReady();

            Assert.AreEqual(playerNum1, _netLockClient1.PlayerNumber, "Server maintains the same player num for the same client.");
            Assert.AreEqual(playerNum2, _netLockClient2.PlayerNumber, "Server maintains the same player num for the same client.");
            Assert.AreEqual(playerNum3, _netLockServer.LocalPlayerNumber, "Server maintains the same player num for the same client.");

            _netLockServer.UnregisterLocalClient();
            _netClient1.Disconnect();
            _netClient2.Disconnect();

            _netLockServer.RegisterLocalClient(localClient, _factory);
            _netLockServer.LocalPlayerReady();

            _netClient1.Connect();
            _netLockClient1.SendPlayerReady();

            _netClient2.Connect();
            _netLockClient2.SendPlayerReady();

            Assert.AreEqual(playerNum1, _netLockClient1.PlayerNumber, "Server maintains the same player num for the same client.");
            Assert.AreEqual(playerNum2, _netLockClient2.PlayerNumber, "Server maintains the same player num for the same client.");
            Assert.AreEqual(playerNum3, _netLockServer.LocalPlayerNumber, "Server maintains the same player num for the same client.");
        }

        [Test]
        public void ReconnectSituation()
        {
            StartMatch();

            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);

            _netClient1.Disconnect();

            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);
            Update(100);

            _lockClient2.AddPendingCommand(new TestCommand(4));

            Update(100);
            Update(100);
            Update(100);

            Assert.AreEqual(0, _logic1.SumValues, "Client is disconnected, it did not get the command");
            Assert.AreEqual(0, _lockClient1.TurnBuffer, "Client is disconnected, no turn buffer");

            _netClient1.Connect();

            Assert.AreEqual(0, _logic1.SumValues, "Client is reconnected, but has not applied command");

            Update(0);

            Assert.AreEqual(0, _logic1.SumValues, "Client is reconnected, but has not sent player ready");

            Assert.AreEqual(0, _lockClient1.TurnBuffer, "Client is reconnected, but has not sent player ready");

            _netLockClient1.SendPlayerReady();

            Update(0);

            Assert.AreEqual(14, _netLockServer.CurrentTurnNumber, "Client is reconnected, server resends all turns from the beginning");
            Assert.AreEqual(9, _lockClient1.TurnBuffer, "Client is reconnected, server resends all turns from the beginning");

            Update(0);
            Update(0);
            Update(0);
            Update(0);

            Assert.AreEqual(5, _lockClient1.TurnBuffer, "Client is reconnected and playing, but has not catched up");
            Assert.AreEqual(0, _logic1.SumValues, "Client is reconnected and playing, but has not catched up");

            Update(0);
            Update(0);
            Update(0);

            Assert.AreEqual(4, _logic1.SumValues, "Client is reconnected, client simulation catched up");
        }
    }

}