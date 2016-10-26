using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.WebSockets
{
    public class WebSocketsEventDispatcher : IDisposable
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
            public string Error;

            public EventData(EventType type)
            {
                Type = type;
                Message = null;
                Error = null;
            }

            public EventData(string message, string error = null)
            {
                Type = EventType.Error;
                Message = message;
                Error = error;
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

        readonly ICoroutineRunner _runner;
        List<EventData> _pending;
        IEnumerator _dispatchCoroutine;

        public WebSocketsEventDispatcher(ICoroutineRunner runner)
        {
            _pending = new List<EventData>();
            _runner = runner;

            _dispatchCoroutine = _runner.StartCoroutine(Dispatch());
        }

        public void Dispose()
        {
            if(_dispatchCoroutine != null)
            {
                _runner.StopCoroutine(_dispatchCoroutine);
                _dispatchCoroutine = null;
            }
            _pending.Clear();
        }

        IEnumerator Dispatch()
        {
            while(true)
            {
                if(_pending.Count > 0)
                {
                    var events = _pending;
                    _pending = new List<EventData>();

                    for(int i = 0; i < events.Count; ++i)
                    {
                        var e = events[i];
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

                yield return null;
            }
        }

        public void NotifyConnected()
        {
            _pending.Add(new EventData(EventType.Open));
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
            _pending.Add(new EventData(EventType.Close));
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
            _pending.Add(new EventData(msg));
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
            _pending.Add(new EventData(null, error));
        }

        void DispatchNetworkError(string message)
        {
            if(OnNetworkError != null)
            {
                OnNetworkError(new Error(message));
            }
        }
    }
}
