using System;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.WAMP;
using SocialPoint.WAMP.Subscriber;
using SocialPoint.WAMP.Publisher;
using SocialPoint.WAMP.Caller;

namespace SocialPoint.Connection
{
    [Serializable]
    public class ConnectionManagerConfig
    {
        public float PingInterval = 10.0f;
        public float ReconnectInterval = 1.0f;

        public int RPCRetries = 1;
        public float RPCTimeout = 1.0f;
    }

    public delegate void NotificationReceivedDelegate(int type, string topic, AttrDic dictParams);

    public interface IConnectionManager : IDisposable
    {
        event Action OnConnectionStablished;
        event Action OnConnected;
        event Action OnClosed;
        event Action<AttrDic> OnProcessServices;
        event Action<Error> OnError;
        event Action<Error> OnRPCError;
        event NotificationReceivedDelegate OnNotificationReceived;
        event NotificationReceivedDelegate OnPendingNotification;

        ConnectionManagerConfig Config { get; set; }

        IAppEvents AppEvents { set; }

        IUpdateScheduler Scheduler { set; }

        bool IsConnected { get; }

        float RPCTimeout { get; }

        string[] Urls { get; }

        string ConnectedUrl { get; }

        bool DebugEnabled { get; set; }

        ILoginData LoginData { get; set; }

        SocialPoint.Social.IPlayerData PlayerData { get; set; }

        Localization Localization { get; set; }

        IDeviceInfo DeviceInfo { get; set; }

        void AutosubscribeToTopic(string topic, Subscription subscription);

        WAMPConnection.StartRequest Connect();

        WAMPConnection.StartRequest Reconnect();

        void Disconnect();

        PublishRequest Publish(string topic, AttrList args, AttrDic kwargs, OnPublished onComplete);

        CallRequest Call(string procedure, AttrList args, AttrDic kwargs, HandlerCall onResult);
    }
}
