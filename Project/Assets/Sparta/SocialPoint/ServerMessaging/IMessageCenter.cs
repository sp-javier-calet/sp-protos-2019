using System;
using System.Collections.Generic;

using SocialPoint.Base;

namespace SocialPoint.ServerMessaging
{
    public interface IMessageCenter
    {
        List<Message> Messages{get;}
        void SendMessage(Message message, Action<Error> cbk = null);
        void DeleteMessages(List<Message> messages, Action<Error> cbk = null);
        void RequestMessages(Action<List<Message>,Error> cbk = null);
    }
}

