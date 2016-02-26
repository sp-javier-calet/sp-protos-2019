using System;
using System.Collections.Generic;

using SocialPoint.Base;

namespace SocialPoint.ServerMessaging
{
    public interface IMessageCenter : IDisposable
    {
        IEnumerator<Message> Messages{ get; }

        event Action<Error> ErrorEvent;
        event Action<IMessageCenter> UpdatedEvent;

        void Load();

        void SendMessage(Message message);

        void DeleteMessages(List<Message> messages);
    }

    public static class MessageCenterExtensions
    {
        public static void DeleteMessage(this IMessageCenter msgs, Message msg)
        {
            var list = new List<Message>();
            list.Add(msg);
            msgs.DeleteMessages(list);
        }
    }
}

