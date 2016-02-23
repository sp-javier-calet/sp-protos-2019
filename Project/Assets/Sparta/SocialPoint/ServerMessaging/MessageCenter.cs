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
        List<Message> _messages;

        const string GetMessagesCommandName = "messages.get";
        const string SendMessagesCommandName = "messages.send";
        const string DeleteMessagesCommandName = "messages.delete";
        const string PendingMessagesCommandName = "messages.pending";
        const string MessagesArgName = "msgs";

        public MessageCenter(ICommandQueue commandQueue, CommandReceiver commandReceiver)
        {
            _messages = new List<Message>();
            _commandQueue = commandQueue;
            _commandReceiver = commandReceiver;
            _commandReceiver.RegisterCommand(PendingMessagesCommandName, (cmd) => ParseMessages(cmd.Args));
        }

        #region IMessageCenter implementation

        public void RequestMessages(Action<List<Message>,Error> cbk = null)
        {
            _commandQueue.Add(new Command(GetMessagesCommandName), (attr, error) =>  ParseResponseGetMessagesCommand(attr, error, cbk));
        }

        public void SendMessage(Message message, Action<Error> cbk = null)
        {
            _commandQueue.Add(new Command(SendMessagesCommandName, message.ToAttr()), (resp, err) => cbk(err));
        }

        public void DeleteMessages(List<Message> messages, Action<Error> cbk = null)
        {
            var arg = new AttrList();
            messages.ForEach((message) => arg.Add(new AttrString(message.Id)));
            _commandQueue.Add(new Command(DeleteMessagesCommandName, arg), (resp, err) => {
                foreach(var messageId in arg)
                {
                    _messages.Remove(_messages.Find(m => m.Id == messageId.ToString()));
                }
                cbk(err);
            });
        }

        public List<Message> Messages
        {
            get
            {
                return _messages;
            }
        }

        #endregion

        void ParseMessages(Attr data)
        {
            var dataList = data.AsList;
            foreach(var messageData in dataList)
            {
                var message = new Message(messageData.AsDic);
                if(!_messages.Exists(m => m.Id == message.Id))
                {
                    _messages.Add(message);
                }
            }
        }

        void ParseResponseGetMessagesCommand(Attr data, Error error, Action<List<Message>,Error> cbk = null)
        {
            if(!Error.IsNullOrEmpty(error))
            {
                //TODO: do something
                return;
            }
            var dataDic = data.AsDic;
            ParseMessages(dataDic.GetValue(MessagesArgName));
            if(cbk != null)
            {
                cbk(_messages, error);
            }
        }
    }
}