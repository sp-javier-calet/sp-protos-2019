using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class CommandReceiver 
    {
        public delegate void CommandCallback(STCCommand cmd);

        Action<STCCommand> OnCommandExecuted;

        readonly Dictionary<string, ISTCCommandFactory> _registeredCommands;

        public CommandReceiver()
        {
            _registeredCommands = new Dictionary<string, ISTCCommandFactory>();
        }

        // Private Listener. Used by admin panel
        void SetListener(Action<STCCommand> listener)
        {
            OnCommandExecuted = listener;
        }

        public bool Receive(AttrDic data, out string commandId)
        {
            commandId = STCCommand.getId(data);
            var name = STCCommand.getName(data);
            ISTCCommandFactory fct;

            if(_registeredCommands.TryGetValue(name, out fct))
            {
                var command = fct.Create(data);

                if(OnCommandExecuted != null)
                {
                    OnCommandExecuted(command);
                }

                command.Exec();

                return true;
            }

            return false;
        }

        public void RegisterCommand(string name, ISTCCommandFactory factory)
        {
            if(factory == null)
            {
                throw new ArgumentNullException("factory", "Invalid Command Factory");
            }

            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Invalid command name");
            }

            if(_registeredCommands.ContainsKey(name))
            {
                throw new InvalidOperationException("Already registered STC Command");
            }

            _registeredCommands.Add(name, factory);
        }

        public void RegisterCommand(string name, CommandCallback cbk)
        {
            RegisterCommand(name, new CallbackSTCCommand.Factory(name, cbk));
        }

        public void UnregisterCommand(string name)
        {
            _registeredCommands.Remove(name);
        }


        #region Private Callback Command implementation

        class CallbackSTCCommand : STCCommand
        {
            readonly CommandCallback _callback;

            CallbackSTCCommand(string name, AttrDic data, CommandCallback cbk)
                : base(name, data)
            {
                _callback = cbk;
            }

            public override void Exec()
            {
                _callback(this);
            }

            public sealed class Factory : ISTCCommandFactory
            {
                readonly string _name;
                readonly CommandCallback _callback;

                public Factory(string name, CommandCallback cbk)
                {
                    _name = name;
                    _callback = cbk;
                }

                public STCCommand Create(AttrDic data)
                {
                    return new CallbackSTCCommand(_name, data, _callback);
                }
            }
        }

        #endregion
    }
}
