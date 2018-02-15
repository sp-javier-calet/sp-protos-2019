using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    [TestFixture]
    [Category("SocialPoint.Lockstep")]
    class LockstepClientTests
    {
        LockstepClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = new LockstepClient();
        }

        [Test]
        public void StartedCalled()
        {
            bool started = false;
            _client.SimulationStarted += () => started = true;
            _client.Update(100);
            Assert.IsFalse(started, "Started should not be called withouth having called Start()");
            _client.Start();
            Assert.IsFalse(started, "Started should not be called directly on Start()");
            _client.Update(0);
            Assert.IsTrue(started, "Started should be called on first Update() after Start()");
            _client.Stop();
            started = false;
            _client.Update(100);
            Assert.IsFalse(started, "Started should not be called after Stop()");
            _client.Start();
            Assert.IsFalse(started);
            _client.Update(2000);
            Assert.IsTrue(started);
            started = false;
            _client.Stop();
            _client.Start(-200);
            _client.Update(150);
            Assert.IsFalse(started, "Started should not be called if update time is lower than start delay");
            _client.Update(150);
            Assert.IsTrue(started, "Started should be called after update time exceeds start delay");

            started = false;
            _client.Stop();
            _client.Start(200);
            _client.Update(0);
            Assert.IsTrue(started, "Started should not be called if started after 0 time (reconnection)");
        }

        [Test]
        public void SimulateCalled()
        {
            long time = 0;
            int times = 0;
            _client.Config.SimulationStepDuration = 100;
            _client.ClientConfig.MaxSimulationStepsPerFrame = 10;
            _client.Simulate += (dt) => { times++; time += dt; };

            _client.Update(200);
            Assert.AreEqual(0, time, "Simulate should not be called when client is stopped");

            _client.Start();

            _client.Update(50);
            Assert.AreEqual(0, time, "Simulate should not be called if update time does not exceed SimulationStepDuration");

            _client.Update(200);
            Assert.AreEqual(2, times, "Simulate should be called on each simulation step");
            Assert.AreEqual(200, time, "Simulate should be called with the fixed step dt");

            _client.Update(149);
            Assert.AreEqual(300, time, "Simulate should only be called on complete simulation steps");

            _client.Update(50);
            Assert.AreEqual(400, time, "Client should store the last simulation step to add update dts");

            _client.ClientConfig.SpeedFactor = 2.0f;
            _client.Update(50);
            Assert.AreEqual(500, time, "SpeedFactor will multiply time to accelerate steps");
            _client.ClientConfig.SpeedFactor = 0.5f;
            _client.Update(50);
            Assert.AreEqual(500, time);
            _client.Update(160);
            Assert.AreEqual(600, time);
            _client.Update(2000);
            Assert.AreEqual(1600, time, "Client will only call MaxSimulationStepsPerFrame each frame");
        }

        [Test]
        public void CommandsProcessed()
        {
            _client.Config.CommandStepDuration = 100;
            _client.ClientConfig.LocalSimulationDelay = 0;

            var cmd = Substitute.For<ILockstepCommand>();
            var apply = Substitute.For<ILockstepCommandLogic>();
            var finish = Substitute.For<ILockstepCommandLogic>();

            _client.RegisterCommandLogic(cmd.GetType(), apply);
            _client.AddPendingCommand(cmd, finish);
            _client.Update(1000);
            // finish called because client not started
            finish.Received().Apply(cmd, _client.PlayerNumber);
            apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());
            finish = Substitute.For<ILockstepCommandLogic>();
            _client.Start();
            _client.AddPendingCommand(cmd, finish);
            _client.Update(50);
            finish.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());
            _client.Update(50);
            finish.Received().Apply(cmd, _client.PlayerNumber);
            apply.Received().Apply(cmd, _client.PlayerNumber);

            cmd = Substitute.For<ILockstepCommand>();
            apply = Substitute.For<ILockstepCommandLogic>();
            finish = Substitute.For<ILockstepCommandLogic>();

            _client.ClientConfig.LocalSimulationDelay = 1000;
            _client.Stop();
            _client.Start();
            _client.RegisterCommandLogic(cmd.GetType(), apply);
            _client.AddPendingCommand(cmd, finish);
            _client.Update(950);
            finish.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());
            _client.Update(149);
            finish.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());
            apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());
            _client.Update(1);
            finish.Received().Apply(cmd, _client.PlayerNumber);
            apply.Received().Apply(cmd, _client.PlayerNumber);

        }

        [Test]
        public void ConnectionChangesDetected()
        {
            bool connChanged = false;
            _client.StateChanged += () =>  connChanged = true;
            // add delegate to simulate server that needs to confirm commands
            _client.CommandAdded += delegate {};
            _client.ClientConfig.LocalSimulationDelay = 1000;
            _client.Config.CommandStepDuration = 100;
            _client.Config.SimulationStepDuration = 1000;

            _client.Start(-1000);
            Assert.IsTrue(_client.Connected, "Client should be connected after start.");
            _client.Update(100);
            Assert.IsTrue(_client.Connected, "Client should be connected if update time is less than start delay.");
            _client.Update(1000);
            Assert.IsTrue(connChanged);
            Assert.IsFalse(_client.Connected, "Client should be disconnected if update time exceeds start delay without having any turn data.");

            connChanged = false;

            _client.AddConfirmedTurn(new ClientTurnData());
            _client.Update(100);
            Assert.IsFalse(connChanged);
            Assert.IsFalse(_client.Connected, "Client should not be reconnected if it did not receive enough turns.");

            _client.AddConfirmedTurn(new ClientTurnData());
            _client.Update(0);
            Assert.IsTrue(connChanged);
            Assert.IsTrue(_client.Connected, "Client should be reconnected if it received enough turns.");
        }

        [Test]
        public void ConnectionChangesDetectedGracefully()
        {
            bool connChanged = false;
            _client.StateChanged += () =>  connChanged = true;
            // add delegate to simulate server that needs to confirm commands
            _client.CommandAdded += delegate {};
            _client.ClientConfig.LocalSimulationDelay = 1000;
            _client.Config.CommandStepDuration = 100;
            _client.Config.SimulationStepDuration = 1000;

            _client.Start(-1000);
            _client.Update(1100);
            connChanged = false;
            _client.AddConfirmedTurn(new ClientTurnData());
            _client.Update(200);
            _client.AddConfirmedTurn(new ClientTurnData());
            _client.Update(0);
            Assert.IsFalse(connChanged);
            Assert.IsFalse(_client.Connected, "Client is waiting until it has enough turns to go to current simulation time.");
            _client.AddConfirmedTurn(new ClientTurnData());
            _client.Update(0);
            Assert.IsTrue(connChanged);
            Assert.IsTrue(_client.Connected, "Client should be reconnected if it received enough turns.");
        }
    }

}