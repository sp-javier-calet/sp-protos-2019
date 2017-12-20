using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.ServerMessaging
{
    public sealed class EmptyMessageCenter : IMessageCenter
    {
        List<Message> _messages;

        public EmptyMessageCenter()
        {
            _messages = new List<Message>();
        }

        #region IMessageCenter implementation

        public event Action<IMessageCenter> UpdatedEvent
        {
            add { }
            remove { }
        }

        public void UpdateMessages(Action<Error> callback = null)
        {
        }

        public void SendMessage(Message message, Action<Error> callback = null)
        {
        }

        public void ReadMessages(List<Message> messages, Action<Error> callback = null)
        {
        }

        public void DeleteMessages(List<Message> messages, Action<Error> callback = null)
        {
        }

        public IEnumerator<Message> Messages
        {
            get
            {
                return _messages.GetEnumerator();
            }
        }

        public void Dispose()
        {
        }

        #endregion

    }
}

