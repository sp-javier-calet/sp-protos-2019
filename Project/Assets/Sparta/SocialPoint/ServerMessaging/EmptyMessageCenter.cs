
using System;
using System.Collections.Generic;

namespace SocialPoint.ServerMessaging
{
    public class EmptyMessageCenter : IMessageCenter
    {
        List<Message> _messages;

        public EmptyMessageCenter()
        {
            _messages = new List<Message>();
        }

        #region IMessageCenter implementation

        public event Action<SocialPoint.Base.Error> ErrorEvent
        {
            add { }
            remove { }
        }

        public event Action<IMessageCenter> UpdatedEvent
        {
            add { }
            remove { }
        }

        public void Load()
        {
        }

        public void SendMessage(Message message)
        {
        }

        public void DeleteMessages(List<Message> messages)
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

