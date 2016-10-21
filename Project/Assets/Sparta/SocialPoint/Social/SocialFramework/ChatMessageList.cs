using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public interface IMessageList
    {
        IEnumerator<IChatMessage> GetMessages();

        int Add(IChatMessage message);

        void Edit(int index, Action<IChatMessage> editCallback);

        void SetHistory(IEnumerable<IChatMessage> historic);

        void ProcessMessages(Action<int, IChatMessage> processCallback);

        void Clear();
    }

    public class ChatMessageList<MessageType> : IMessageList where MessageType : class, IChatMessage
    {
        public delegate void MessageListEventDelegate();
        public delegate void MessageEventDelegate(int index);

        public event MessageListEventDelegate OnMessagesCleared;
        public event MessageListEventDelegate OnHistoryAdded;
        public event MessageEventDelegate OnMessageAdded;
        public event MessageEventDelegate OnMessageEdited;

        readonly List<MessageType> _messages;

        public bool HasMessages
        {
            get
            {
                return _messages.Count > 0;
            }
        }

        public int Count
        {
            get
            {
                return _messages.Count;
            }
        }

        public ChatMessageList()
        {
            _messages = new List<MessageType>();
        }

        public IEnumerator<IChatMessage> GetMessages()
        {
            for(int i = 0; i < _messages.Count; ++i)
            {
                yield return _messages[i];
            }
        }

        public IEnumerator<MessageType> GetCustomMessages()
        {
            for(int i = 0; i < _messages.Count; ++i)
            {
                yield return _messages[i];
            }
        }

        public int Add(IChatMessage message)
        {
            var custom = message as MessageType;
            if(custom == default(MessageType))
            {
                throw new Exception("Incompatible message type");
            }
            return Add(custom);
        }

        public int Add(MessageType message)
        {
            _messages.Add(message);
            var idx = _messages.Count - 1;
            if(OnMessageAdded != null)
            {
                OnMessageAdded(idx);
            }
            return idx;
        }

        public void Edit(int index, Action<IChatMessage> editCallback)
        {
            Edit(index, new Action<MessageType>(editCallback));
        }

        public void Edit(int index, Action<MessageType> editCallback)
        {
            if(index < 0 || index >= _messages.Count)
            {
                Log.e("Invalid index");
                return;
            }

            var msg = _messages[index];
            editCallback(msg);

            if(OnMessageEdited != null)
            {
                OnMessageEdited(index);
            }
        }

        public void SetHistory(IEnumerable<IChatMessage> historic)
        {
            var custom = historic as IEnumerable<MessageType>;
            if(custom == default(IEnumerable<MessageType>))
            {
                Log.e("Incompatible message type");
                return;
            }

            SetHistory(custom);
        }

        public void SetHistory(IEnumerable<MessageType> historic)
        {
            Clear();
            _messages.AddRange(historic);

            if(OnHistoryAdded != null)
            {
                OnHistoryAdded();
            }
        }

        public void ProcessMessages(Action<int, IChatMessage> processCallback)
        {
            ProcessMessages(new Action<int, MessageType>(processCallback));
        }

        public void ProcessMessages(Action<int, MessageType> processCallback)
        {
            for(int i = 0; i < _messages.Count; ++i)
            {
                processCallback(i, _messages[i]);
            }
        }

        public void Clear()
        {
            _messages.Clear();

            if(OnMessagesCleared != null)
            {
                OnMessagesCleared();
            }
        }
    }
}
