using System.Collections;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;

namespace SocialPoint.WAMP
{
    public interface IWAMPConnectionDelegate
    {
        void OnClientConnected();

        void OnClientDisconnected();

        void OnNetworkError(Error err);
    }

    public class WAMPConnection
    {
        public delegate void OnCompleted();

        public delegate void HandlerSubscription(AttrList args, AttrDic kwargs);

        public delegate void HandlerPublication(WAMPConnection.Publication pub);

        public class Subscription
        {
            public ulong Id{ get; private set; }

            public string Topic{ get; private set; }

            public Subscription(ulong id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        public class Publication
        {
            public ulong Id{ get; private set; }

            public string Topic{ get; private set; }

            public Publication(ulong id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        const string NoSessionErrorTag = "no_session_error";
        const string ProtocolErrorTag = "protocol_error";



        enum ErrorCodes
        {
            NoSession = 1001,
            JoinInProgress = 1002,
            LeaveInProgress = 1003,
            SessionAborted = 1004,
            CallError = 1005,
            PublishError = 1006,
            SubscribeError = 1007,
            UnsubscribeError = 1008,
            ConnectionClosed = 1009,
        }

        INetworkClient _networkClient;

        WAMPConnection(INetworkClient networkClient)
        {
            _networkClient = networkClient;
        }

        OnCompleted _startCompletionHandler;

        public void start(OnCompleted completionHandler)
        {
            _startCompletionHandler = completionHandler;
        }

        OnCompleted _stopCompletionHandler;

        public void stop(OnCompleted completionHandler)
        {
            _stopCompletionHandler = completionHandler;
        }

        public void addConnectionStateDelegate(IWAMPConnectionDelegate dlg)
        {

        }
    }
}
