using NUnit.Framework;
using NSubstitute;
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Lockstep
{
    [TestFixture]
    [Category("SocialPoint.Lockstep")]
    class LockstepReplayTests
    {
        ClientLockstepController _client;
        LockstepReplay _replay;
        LockstepCommandFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _client = new ClientLockstepController();
            _factory = new LockstepCommandFactory();
            var cmd = Substitute.For<ILockstepCommand>();
            cmd.Clone().Returns(cmd);
            _factory.Register<ILockstepCommand>(1, cmd);
            _replay = new LockstepReplay(_client, _factory);
        }

        [Test]
        public void RecordWorking()
        {
            _replay.Record();
            Assert.AreEqual(0, _replay.CommandCount);
            _client.Start();
            var cmd = Substitute.For<ILockstepCommand>();
            _client.AddPendingCommand(cmd);
            Assert.AreEqual(0, _replay.CommandCount);
            _client.Update(2000);
            Assert.AreEqual(1, _replay.CommandCount);
        }

        [Test]
        public void SerializeWorking()
        {
            _replay.Record();
            _client.Start();
            var cmd = Substitute.For<ILockstepCommand>();
            _client.AddPendingCommand(cmd);
            _client.Update(2000);

            var stream = new MemoryStream();
            var writer = new SystemBinaryWriter(stream);
            _replay.Serialize(writer);
            stream.Seek(0, SeekOrigin.Begin);
            var reader = new SystemBinaryReader(stream);

            _replay.Clear();
            _replay.Deserialize(reader);

            Assert.AreEqual(1, _replay.CommandCount);
        }

        [Test]
        public void ReplayWorking()
        {
            var cmd = Substitute.For<ILockstepCommand>();
            var finish = Substitute.For<ILockstepCommandLogic>();
            _replay.AddCommand(0, cmd, finish);
            _replay.Replay();
            _client.Start();
            finish.DidNotReceive().Apply(Arg.Any<ILockstepCommand>());
            _client.Update(2000);
            finish.Received().Apply(cmd);
        }
    }

}