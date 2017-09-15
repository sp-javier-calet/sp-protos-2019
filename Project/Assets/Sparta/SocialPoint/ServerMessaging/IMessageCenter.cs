using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.ServerMessaging
{
    public interface IMessageCenter : IDisposable
    {
        IEnumerator<Message> Messages{ get; }

        event Action<IMessageCenter> UpdatedEvent;

        void UpdateMessages(Action<Error> callback = null);

        void SendMessage(Message message, Action<Error> callback = null);

        void ReadMessages(List<Message> messages, Action<Error> callback = null);

        void DeleteMessages(List<Message> messages, Action<Error> callback = null);
    }

    public static class MessageCenterExtensions
    {
        public static void DeleteMessage(this IMessageCenter msgs, Message msg, Action<Error> callback = null)
        {
            var list = new List<Message>();
            list.Add(msg);
            msgs.DeleteMessages(list, callback);
        }

        public static void ReadMessage(this IMessageCenter msgs, Message msg, Action<Error> callback = null)
        {
            var list = new List<Message>{msg};
            msgs.ReadMessages(list, callback);
        }
    }
}

