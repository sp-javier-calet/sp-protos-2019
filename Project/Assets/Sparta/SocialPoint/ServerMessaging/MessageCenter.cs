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
        const string PendingMessagesCommandName = "messages.pending";
        const string MessagesArgName = "msgs";

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
            var arg = new AttrList();

            messages.ForEach((message) => {
                if(!_messagesPendingDelete.Contains(message.Id))
                {
                    arg.Add(new AttrString(message.Id));
                    _messagesPendingDelete.Add(message.Id);
                }
            });

            _commandQueue.Add(new Command(DeleteMessagesCommandName, arg, false, false), (resp, err) => {
                if(Error.IsNullOrEmpty(err))
                {
                    foreach(var messageId in arg)
                    {
                        _messages.Remove(messageId.ToString());
                        _messagesPendingDelete.Remove(messageId.ToString());
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
                foreach(var message in _messagesPendingDelete)
                {
                    a.Remove(a.Find(m => m.Id == message));
                }
                return a.GetEnumerator();
            }
        }

        #endregion

        void ParseMessages(Attr data)
        {
            var dataList = data.AsList;
            var newMessages = false;
            foreach(var messageData in dataList)
            {
                var message = new Message(messageData.AsDic);
                if(!_messages.ContainsKey(message.Id))
                {
                    _messages.Add(message.Id, message);
                    newMessages = true;
                }
            }
            if(newMessages)
            {
                UpdatedEvent(this);
            }
        }

        void ParseResponseGetMessagesCommand(Attr data, Error err)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                ErrorEvent(err);
                return;
            }
            var dataDic = data.AsDic;
            ParseMessages(dataDic.GetValue(MessagesArgName));
        }
    }
}