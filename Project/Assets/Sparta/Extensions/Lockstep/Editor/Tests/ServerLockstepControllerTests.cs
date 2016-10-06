using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    [TestFixture]
    [Category("SocialPoint.Lockstep")]
    class ServerLockstepControllerTests
    {
        ServerLockstepController _server;

        [SetUp]
        public void SetUp()
        {
            _server = new ServerLockstepController();
        }

        [Test]
        public void TurnReadyCalled()
        {
            int readyCalled = 0;
            _server.TurnReady += (turn) =>  readyCalled++;
            _server.Config.CommandStepDuration = 200;

            var cmd1 = new ServerLockstepCommandData();
            _server.AddCommand(cmd1);
            var cmd2 = new ServerLockstepCommandData();
            _server.AddCommand(cmd2);

            _server.Update(1000);

            Assert.AreEqual(0, readyCalled, "Server should not close turns if not started");

            _server.Start();

            _server.Update(100);

            Assert.AreEqual(0, readyCalled, "Server should call TurnReady on each command step");

            _server.Update(300);

            Assert.AreEqual(2, readyCalled, "Server should call TurnReady on each command step");
        }

        [Test]
        public void LocalClientWorking()
        {
            var client = new ClientLockstepController();
            var factory = new LockstepCommandFactory();
            var cmd = Substitute.For<ILockstepCommand>();
            cmd.Clone().Returns(cmd);

            factory.Register<ILockstepCommand>(1, cmd);                
            _server.RegisterLocalClient(client, factory);
            _server.Config.CommandStepDuration = 100;

            _server.Start();
            client.Start(1000);

            _server.Update(1000);
            client.Update(1000);

            var finish = Substitute.For<ILockstepCommandLogic>();
            client.AddPendingCommand(cmd, finish);

            _server.Update(100);
            client.Update(100);

            finish.DidNotReceive().Apply(Arg.Any<ILockstepCommand>());

            _server.Update(1000);
            client.Update(1000);

            finish.Received().Apply(cmd);
        }
    }

}