using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    class CommandReceiverTest
    {
        const string FactoryCommandName = "factory_command";
        const string CallbackCommandName = "callback_command";

        const string CommandId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

        CommandReceiver CommandReceiver;

        STCCommand Command;
        CommandReceiver.CommandCallback Callback;

        [SetUp]
        public void SetUp()
        {
            CommandReceiver = new CommandReceiver();

            // Register test commands
            Command = Substitute.For<STCCommand>(FactoryCommandName, CreateCommand(FactoryCommandName));
            ISTCCommandFactory factory = Substitute.For<ISTCCommandFactory>();
            factory.Create(Arg.Any<AttrDic>()).Returns(Command);
            CommandReceiver.RegisterCommand(FactoryCommandName, factory);

            Callback = Substitute.For<CommandReceiver.CommandCallback>();
            CommandReceiver.RegisterCommand(CallbackCommandName, Callback);
        }

        static AttrDic CreateCommand(string name)
        {
            var attr = new AttrDic();
            attr.Set("cid", new AttrString(CommandId));
            attr.Set("cmd", new AttrString(name));
            attr.Set("ts", new AttrLong(TimeUtils.Timestamp));
            attr.Set("args", new AttrDic());
            return attr;
        }

        [Test]
        public void Register_repeated_error()
        {
            try
            {
                CommandReceiver.RegisterCommand(CallbackCommandName, Callback);
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Register_invalid_command_error()
        {
            try
            {
                CommandReceiver.RegisterCommand(string.Empty, Callback);
            }
            catch(ArgumentNullException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Register_null_factory_error()
        {
            try
            {
                ISTCCommandFactory factory = null;
                CommandReceiver.RegisterCommand(CallbackCommandName, factory);
            }
            catch(ArgumentNullException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void Receive_unknown_command()
        {
            string commandId;
            var result = CommandReceiver.Receive(CreateCommand("unknown_command"), out commandId);

            Assert.IsFalse(result);
            Assert.AreEqual(CommandId, commandId);
        }

        [Test]
        public void Receive_unregistered_command()
        {
            CommandReceiver.UnregisterCommand(CallbackCommandName);

            string commandId;
            var result = CommandReceiver.Receive(CreateCommand(CallbackCommandName), out commandId);

            Assert.IsFalse(result);
            Assert.AreEqual(CommandId, commandId);
        }

        [Test]
        public void Receive_factory_command()
        {
            string commandId;
            var result = CommandReceiver.Receive(CreateCommand(FactoryCommandName), out commandId);

            Command.Received(1).Exec();
            Assert.IsTrue(result);
            Assert.AreEqual(CommandId, commandId);
        }

        [Test]
        public void Receive_callback_command()
        {
            string commandId;
            var result = CommandReceiver.Receive(CreateCommand(CallbackCommandName), out commandId);

            Callback.Received(1).Invoke(Arg.Any<STCCommand>());
            Assert.IsTrue(result);
            Assert.AreEqual(CommandId, commandId);
        }
    }
}