using NSubstitute;
using NUnit.Framework;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    [TestFixture]
    internal class LifecycleLockstepNetworkSceneTests
    {
        [SetUp]
        public void SetUp()
        {
            _updateScheduler = new UpdateScheduler();
        }

        const float DeltaTimeTest = 0.033f;
        const int SimStep = 33;
        const int CmdStep = 198;
        const bool RealtimeUpdate = false;

        LifecycleController _controller;
        UpdateScheduler _updateScheduler;

        NetworkGameObject _ngo1;
        NetworkGameObject _ngo2;

        INetworkSceneDelegate _networkSceneDelegate1;
        INetworkSceneDelegate _networkSceneDelegate2;

        INetworkBehaviour _networkBehaviour1;
        INetworkBehaviour _networkBehaviour2;

        ILockstepCommand _cmd;
        ILockstepCommandLogic _apply;
        ILockstepCommandLogic _clientApply;

        LockstepClientLifecycleComponent _lockstepController;

        void SetupLifecycleComponents(bool scheduledUpdate = true)
        {
            var config = new ClientConfig();
            config.General = new Config();
            config.General.SimStep = SimStep;
            config.General.CmdStep = CmdStep;
            config.General.RealtimeUpdate = RealtimeUpdate;

            _lockstepController = new LockstepClientLifecycleComponent(scheduledUpdate ? _updateScheduler : null, config);
            _lockstepController.Lockstep.ClientConfig.LocalSimulationDelay = 0;

            _networkSceneDelegate1 = Substitute.For<INetworkSceneDelegate>();
            _lockstepController.SceneController.Scene.RegisterDelegate(_networkSceneDelegate1);
            _networkSceneDelegate2 = Substitute.For<INetworkSceneDelegate>();
            _lockstepController.SceneController.Scene.RegisterDelegate(_networkSceneDelegate2);

            _controller = new LifecycleController(_updateScheduler);
            _controller.Start();

            _controller.RegisterComponent(_lockstepController);

            // Sets up Lockstep
            _updateScheduler.Update(DeltaTimeTest, DeltaTimeTest);
            // Starts Lockstep
            _updateScheduler.Update(DeltaTimeTest, DeltaTimeTest);
            // Sets up SceneController and first lockstep simulate call
            _updateScheduler.Update(DeltaTimeTest, DeltaTimeTest);
        }

        void ValidateNetworkSceneBehaviours()
        {
            _networkSceneDelegate1.Received(1).Update(33);
            _networkSceneDelegate2.Received(1).Update(33);
        }

        void SetupNGOs(NetworkScene scene)
        {
            _ngo1 = scene.InstantiateObject(2);
            _ngo2 = scene.InstantiateObject(3);

            _networkBehaviour1 = _ngo1.Behaviours.Add(() => Substitute.For<INetworkBehaviour>());
            _networkBehaviour2 = _ngo2.Behaviours.Add(() => Substitute.For<INetworkBehaviour>());
        }

        void SetupCommands()
        {
            _cmd = Substitute.For<ILockstepCommand>();
            _apply = Substitute.For<ILockstepCommandLogic>();
            _clientApply = Substitute.For<ILockstepCommandLogic>();

            var lockstep = _lockstepController.Lockstep;
            lockstep.RegisterCommandLogic(_cmd.GetType(), _clientApply);
        }

        [Test]
        public void LockstepNoServerCommand()
        {
            SetupLifecycleComponents();

            ValidateNetworkSceneBehaviours();

            SetupCommands();
            _lockstepController.Lockstep.AddPendingCommand(_cmd);

            _updateScheduler.Update(DeltaTimeTest, DeltaTimeTest);

            // only 1 more simulation steps happened (total of 2). Not enough to process the command.
            _clientApply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _updateScheduler.Update(DeltaTimeTest * 3, DeltaTimeTest * 3);

            // 3 more simulation steps happened (total of 5). Not enough to process the command.
            _clientApply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _updateScheduler.Update(DeltaTimeTest, DeltaTimeTest);

            // 1 more simulation step happened. Now we have 6, enough to process the next turn and the pending command.
            _clientApply.Received().Apply(_cmd, _lockstepController.Lockstep.PlayerNumber);

            _controller.Dispose();
        }

        // This test uses the scheduler only to process the Lifecycle calls. It does not affect Lockstep and the NetworkScene
        [Test]
        public void LockstepServerCommand()
        {
            var _server = new LockstepServer();

            _server.Config.SimulationStepDuration = SimStep;
            _server.Config.CommandStepDuration = CmdStep;
            _server.Start();

            SetupLifecycleComponents();

            ValidateNetworkSceneBehaviours();

            SetupCommands();

            var factory = new LockstepCommandFactory();
            factory.Register<ILockstepCommand>(1, _cmd);
            _cmd.Clone().Returns(_cmd);

            _server.RegisterLocalClient(_lockstepController.Lockstep, factory);

            // This call should not affect the expected responses as it is not using the scheduler on lockstep
            _updateScheduler.Update(DeltaTimeTest * 10, DeltaTimeTest * 10);

            _server.Update(SimStep);

            _lockstepController.Lockstep.AddPendingCommand(_cmd, _apply);

            _server.Update(SimStep);

            // only 1 more simulation steps happened (total of 2). Not enough to process the command.
            _apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _server.Update(SimStep * 4);

            // 4 more simulation steps happened (total of 6). Not enough to process the command (the local client turn is only processed in the next update).
            _apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _server.Update(SimStep);

            // 1 more simulation step happened. Now we have 6, enough to process the next turn and the pending command.
            _apply.Received().Apply(_cmd, _lockstepController.Lockstep.PlayerNumber);

            _controller.Dispose();
        }

        // This test uses the scheduler only to process the Lifecycle calls. It does not affect Lockstep and the NetworkScene
        [Test]
        public void ServerWithCommandAndNGOs()
        {
            var _server = new LockstepServer();

            _server.Config.SimulationStepDuration = SimStep;
            _server.Config.CommandStepDuration = CmdStep;
            _server.Start();

            SetupLifecycleComponents(false);

            SetupNGOs(_lockstepController.SceneController.Scene);

            SetupCommands();

            var factory = new LockstepCommandFactory();
            factory.Register<ILockstepCommand>(1, _cmd);
            _cmd.Clone().Returns(_cmd);

            _server.RegisterLocalClient(_lockstepController.Lockstep, factory);

            // This call should not affect the expected responses as it is not using the scheduler on lockstep
            _updateScheduler.Update(DeltaTimeTest * 10, DeltaTimeTest * 10);

            _networkSceneDelegate1.DidNotReceive().Update(SimStep);
            _networkSceneDelegate2.DidNotReceive().Update(SimStep);
            _networkBehaviour1.DidNotReceive().Update(SimStep);
            _networkBehaviour2.DidNotReceive().Update(SimStep);

            _server.Update(SimStep);

            _networkSceneDelegate1.Received(1).Update(SimStep);
            _networkSceneDelegate2.Received(1).Update(SimStep);
            _networkBehaviour1.Received(1).Update(SimStep);
            _networkBehaviour2.Received(1).Update(SimStep);

            _lockstepController.Lockstep.AddPendingCommand(_cmd, _apply);

            _server.Update(SimStep);

            // only 1 more simulation steps happened (total of 2). Not enough to process the command.
            _apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _server.Update(SimStep * 4);

            _networkSceneDelegate1.Received(6).Update(SimStep);
            _networkSceneDelegate2.Received(6).Update(SimStep);
            _networkBehaviour1.Received(6).Update(SimStep);
            _networkBehaviour2.Received(6).Update(SimStep);

            // 4 more simulation steps happened (total of 6). Not enough to process the command (the local client turn is only processed in the next update).
            _apply.DidNotReceive().Apply(Arg.Any<ILockstepCommand>(), Arg.Any<byte>());

            _server.Update(33);

            _networkSceneDelegate1.Received(7).Update(SimStep);
            _networkSceneDelegate2.Received(7).Update(SimStep);
            _networkBehaviour1.Received(7).Update(SimStep);
            _networkBehaviour2.Received(7).Update(SimStep);

            // 1 more simulation step happened. Now we have 6, enough to process the next turn and the pending command.
            _apply.Received().Apply(_cmd, _lockstepController.Lockstep.PlayerNumber);

            _controller.Dispose();
        }

        [Test]
        public void Update()
        {
            SetupLifecycleComponents();

            ValidateNetworkSceneBehaviours();

            _controller.Dispose();
        }
    }
}