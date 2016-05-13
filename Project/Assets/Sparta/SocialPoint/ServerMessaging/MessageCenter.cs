using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.ServerSync;

namespace SocialPoint.ServerMessaging
{
    public class MessageCenter : IMessageCenter
    {
        public delegate void GetMessagesDelegate(Error Error, List<Message> messages);

        public const int MsgDontExistError = 1;

        ICommandQueue _commandQueue;
        CommandReceiver _commandReceiver;
        IAppEvents _appEvents;
        Dictionary<string,Message> _messages;
        List<string> _deletedMessages;

        const string GetMessagesCommandName = "messages.get";
        const string SendMessagesCommandName = "messages.send";
        const string DeleteMessagesCommandName = "messages.delete";
        const string DeleteIdsArg = "ids";
        const string PendingMessagesCommandName = "messages.new";
        const string MessagesArg = "msgs";

        public MessageCenter(ICommandQueue commandQueue, CommandReceiver commandReceiver, IAppEvents appEvents)
        {
            _messages = new Dictionary<string,Message>();
            _deletedMessages = new List<string>();
            _commandQueue = commandQueue;
            _commandReceiver = commandReceiver;
            _commandReceiver.RegisterCommand(PendingMessagesCommandName, (cmd) => ParseMessages(cmd.Args));
            _appEvents = appEvents;
            _appEvents.GameWillRestart.Add(0, Reset);
        }

        #region IMessageCenter implementation

        public event Action<IMessageCenter> UpdatedEvent;

        public void UpdateMessages(Action<Error> callback = null)
        {
            _commandQueue.Add(new Command(GetMessagesCommandName), (attr, error) => ParseResponseGetMessagesCommand(attr, error, callback));
        }

        public void SendMessage(Message message, Action<Error> callback = null)
        {
            _commandQueue.Add(new Command(SendMessagesCommandName, message.ToAttr(), false, false), (resp, err) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    if(callback != null)
                    {
                        callback(err);
                    }
                }
            });
        }

        public void DeleteMessages(List<Message> messages, Action<Error> callback = null)
        {
            var ids = new AttrList();

            //check messages exist
            foreach(var message in messages)
            {
                if(_messages.ContainsKey(message.Id))
                {
                    ids.Add(new AttrString(message.Id));
                }
                else
                {
                    callback(new Error(MsgDontExistError, string.Format("message {0} doesn't exist", message.Id)));
                    return;
                }
            }

            //now that we know sure that messages were waiting for deletion
            foreach(var message in messages)
            {
                _messages.Remove(message.Id);
                _deletedMessages.Add(message.Id);
            }

            var handler = UpdatedEvent;
            if(handler != null)
            {
                handler(this);
            }

            var arg = new AttrDic();
            arg.Set(DeleteIdsArg, ids);

            _commandQueue.Add(new Command(DeleteMessagesCommandName, arg, false, false), (resp, err) => {
                if(callback != null)
                {
                    callback(err);
                }
            });
        }

        /// <summary>
        /// Returns all the messages
        /// </summary>
        /// <value>The messages.</value>
        public IEnumerator<Message> Messages
        {
            get
            {
                return _messages.Values.GetEnumerator();
            }
        }


        public void Dispose()
        {
            _commandReceiver.UnregisterCommand(PendingMessagesCommandName);
            _appEvents.GameWillRestart.Remove(Reset);
        }

        #endregion

        /// <summary>
        /// Parses the messages. expects an AttrDict with the key "msgs" and a list of objects message as a value.
        /// </summary>
        /// <param name="data">Data.</param>
        public void ParseMessages(Attr data)
        {
            var messagesList = data.AsDic.Get(MessagesArg).AsList;
            var newMessages = false;
            for(int i = 0; i < messagesList.Count; i++)
            {
                var message = new Message(messagesList[i].AsDic);
                if(!_messages.ContainsKey(message.Id) && !_deletedMessages.Contains(message.Id))
                {
                    _messages.Add(message.Id, message);
                    newMessages = true;
                }
            }
            if(newMessages)
            {
                var handler = UpdatedEvent;
                if(handler != null)
                {
                    handler(this);
                }
            }
        }

        void ParseResponseGetMessagesCommand(Attr data, Error err, Action<Error> callback = null)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                if(callback != null)
                {
                    callback(err);
                }
                return;
            }
            ParseMessages(data);
        }

        void Reset()
        {
            _messages = new Dictionary<string,Message>();
            _deletedMessages = new List<string>();
        }
    }
}