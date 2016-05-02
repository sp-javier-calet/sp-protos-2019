using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Alert;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Locale;
using SocialPoint.GameLoading;
using System;

class CommandQueue : SocialPoint.ServerSync.CommandQueue
{
    public CommandQueue(ICoroutineRunner runner, IHttpClient client) : base(runner, client)
    {
        IgnoreResponses = ServiceLocator.Instance.OptResolve<bool>("command_queue_ignore_responses", IgnoreResponses);
        SendInterval =  ServiceLocator.Instance.OptResolve<int>("command_queue_send_interval", SendInterval);
        MaxOutOfSyncInterval = ServiceLocator.Instance.OptResolve<int>("command_queue_outofsync_interval", MaxOutOfSyncInterval);
        Timeout = ServiceLocator.Instance.OptResolve<float>("command_queue_timeout", Timeout);
        BackoffMultiplier = ServiceLocator.Instance.OptResolve<float>("command_queue_backoff_multiplier", BackoffMultiplier);
        PingEnabled = ServiceLocator.Instance.OptResolve<bool>("command_queue_ping_enabled", PingEnabled);
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        TrackEvent = ServiceLocator.Instance.Resolve<IEventTracker>().TrackEvent;
        RequestSetup = ServiceLocator.Instance.Resolve<ILogin>().SetupHttpRequest;
        CommandReceiver = ServiceLocator.Instance.Resolve<CommandReceiver>();
        AutoSync = ServiceLocator.Instance.Resolve<IGameLoader>().OnAutoSync;
        ServiceLocator.Instance.Resolve<IGameErrorHandler>().Setup(this);
    }
}
