using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.WebSockets
{
    public class WebSocketsEventDispatcher : IUpdateable, IDisposable
    {
        enum EventType
        {
            Open,
            Close,
            Message,
            Error
        }

        struct EventData
        {
            public EventType Type;
            public string Message;
            public Error Error;

            public EventData(EventType type)
            {
                Type = type;
                Message = null;
                Error = null;
            }

            public EventData(string message)
            {
                Type = EventType.Message;
                Message = message;
                Error = null;
            }

            public EventData(Error err)
            {
                Type = EventType.Error;
                Message = null;
                Error = err;
            }
        }

        /// <summary>
        /// Internal events to dispatch
        /// </summary>
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action<NetworkMessageData, IReader> OnMessage;
        public event Action<NetworkMessageData> OnMessageWasReceived;
        public event Action<Error> OnNetworkError;

        readonly IUpdateScheduler _scheduler;
        Queue<EventData> _pending;

        public WebSocketsEventDispatcher(IUpdateScheduler scheduler)
        {
            _pending = new Queue<EventData>();
            _scheduler = scheduler;
            _scheduler.Add(this);
        }

        public void Dispose()
        {
            _scheduler.Remove(this);
            _pending.Clear();
        }

        public void Update()
        {
            if(_pending.Count > 0)
            {
                var events = _pending.Count;
                for(int i = 0; i < events; ++i)
                {
                    var e = _pending.Dequeue();
                    switch(e.Type)
                    {
                    case EventType.Open:
                        DispatchClientConnected();
                        break;

                    case EventType.Close:
                        DispatchClientDisconnected();
                        break;

                    case EventType.Message:
                        DispatchMessageReceived(e.Message);
                        break;

                    case EventType.Error:
                        DispatchNetworkError(e.Error);
                        break;
                    }
                }
            }
        }

        public void NotifyConnected()
        {
            _pending.Enqueue(new EventData(EventType.Open));
        }

        void DispatchClientConnected()
        {
            if(OnClientConnected != null)
            {
                OnClientConnected();
            }
        }

        public void NotifyDisconnected()
        {
            _pending.Enqueue(new EventData(EventType.Close));
        }

        void DispatchClientDisconnected()
        {
            if(OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }
        }

        public void NotifyMessage(string msg)
        {
            _pending.Enqueue(new EventData(msg));
        }

        void DispatchMessageReceived(string data)
        {
            var msg = new NetworkMessageData();

            if(OnMessage != null)
            {
                var reader = new WebSocketsTextReader(data);
                OnMessage(msg, reader);
            }

            if(OnMessageWasReceived != null)
            {
                OnMessageWasReceived(msg);
            }
        }

        public void NotifyError(string error)
        {
            _pending.Enqueue(new EventData(new Error(error)));
        }

        void DispatchNetworkError(Error err)
        {
            if(OnNetworkError != null)
            {
                OnNetworkError(err);
            }
        }
    }
}
