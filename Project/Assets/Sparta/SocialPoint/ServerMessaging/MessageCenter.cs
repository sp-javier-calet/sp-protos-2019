using System;
using System.Collections.Generic;
using SocialPoint.ServerSync;
using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.ServerMessaging
{
    public class MessageCenter : IMessageCenter
    {
        public delegate void GetMessagesDelegate(Error Error,List<Message> messages);

        ICommandQueue _commandQueue;
        CommandReceiver _commandReceiver;
        Dictionary<string,Message> _messages;
        List<string> _messagesPendingDelete;

        const string GetMessagesCommandName = "messages.get";
        const string SendMessagesCommandName = "messages.send";
        const string DeleteMessagesCommandName = "messages.delete";
        const string DeleteIdsArg = "ids";
        const string PendingMessagesCommandName = "messages.new";
        const string MessagesArg = "msgs";

        public MessageCenter(ICommandQueue commandQueue, CommandReceiver commandReceiver)
        {
            _messages = new Dictionary<string,Message>();
            _messagesPendingDelete = new List<string>();
            _commandQueue = commandQueue;
            _commandReceiver = commandReceiver;
            _commandReceiver.RegisterCommand(PendingMessagesCommandName, (cmd) => ParseMessages(cmd.Args));
        }

        #region IMessageCenter implementation

        public event Action<Error> ErrorEvent;

        public event Action<IMessageCenter> UpdatedEvent;

        public void Load()
        {
            _commandQueue.Add(new Command(GetMessagesCommandName), ParseResponseGetMessagesCommand);
        }

        public void SendMessage(Message message)
        {
            _commandQueue.Add(new Command(SendMessagesCommandName, message.ToAttr()), (resp, err) => {
                if(!Error.IsNullOrEmpty(err))
                {
                    ErrorEvent(err);
                }
            });
        }

        public void DeleteMessages(List<Message> messages)
        {
            var arg = new AttrDic();
            var ids = new AttrList();

            for(int i = 0; i < messages.Count; i++)
            {
                if(!_messagesPendingDelete.Contains(messages[i].Id))
                {
                    ids.Add(new AttrString(messages[i].Id));
                    _messagesPendingDelete.Add(messages[i].Id);
                }
            }

            arg.Set(DeleteIdsArg, ids);

            _commandQueue.Add(new Command(DeleteMessagesCommandName, arg, false, false), (resp, err) => {
                if(Error.IsNullOrEmpty(err))
                {
                    for(int i = 0; i < ids.Count; i++)
                    {
                        _messages.Remove(ids[i].ToString());
                        _messagesPendingDelete.Remove(ids[i].ToString());
                    }
                }
                else
                {
                    if(!Error.IsNullOrEmpty(err))
                    {
                        ErrorEvent(err);
                    }
                }
            });
        }

        /// <summary>
        /// Returns all the pending messages
        /// </summary>
        /// <value>The messages.</value>
        public IEnumerator<Message> Messages
        {
            get
            {
                var a = new List<Message>(_messages.Values);
                for(int i = 0; i < _messagesPendingDelete.Count; i++)
                {
                    a.Remove(a.Find(m => m.Id == _messagesPendingDelete[i]));
                }
                return a.GetEnumerator();
            }
        }


        public void Dispose()
        {
            _commandReceiver.UnregisterCommand(PendingMessagesCommandName);
        }

        #endregion

        void ParseMessages(Attr data)
        {
            var messagesList = data.AsDic.Get(MessagesArg).AsList;
            var newMessages = false;
            for(int i = 0; i < messagesList.Count; i++)
            {
                var message = new Message(messagesList[i].AsDic);
                if(!_messages.ContainsKey(message.Id))
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

        void ParseResponseGetMessagesCommand(Attr data, Error err)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                var handler = ErrorEvent;
                if(handler != null)
                {
                    handler(err);
                }
                return;
            }
            ParseMessages(data);
        }
    }
}