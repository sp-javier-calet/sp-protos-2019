//-----------------------------------------------------------------------
// MatchmakingClientLifecycleComponent.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections;
using SocialPoint.Base;
using SocialPoint.Lifecycle;
using SocialPoint.Matchmaking;

public class MatchmakingClientLifecycleComponent : ISetupComponent, ICleanupComponent, ICancelComponent, IMatchmakingClientDelegate, IErrorDispatcher
{
    readonly IMatchmakingClient _matchMakingClient;
    ICancelListener _listener;
    bool _matched;

    public MatchmakingClientLifecycleComponent(IMatchmakingClient matchMakingClient)
    {
        _matchMakingClient = matchMakingClient;
        _matchMakingClient.AddDelegate(this);
    }

    void ICancelComponent.Cancel(ICancelListener listener)
    {
        _listener = listener;
        _matchMakingClient.Stop();
    }

    void ICleanupComponent.Cleanup()
    {
        _matchMakingClient.Stop();
    }

    IErrorHandler IErrorDispatcher.Handler { get; set; }

    void IMatchmakingClientDelegate.OnStart()
    {
    }

    void IMatchmakingClientDelegate.OnStopped(bool successful)
    {
        DispatchStopEvent(successful);
    }

    void IMatchmakingClientDelegate.OnWaiting(int waitTime)
    {
    }

    void IMatchmakingClientDelegate.OnMatched(Match match)
    {
        _matched = true;
    }

    void IMatchmakingClientDelegate.OnError(Error error)
    {
        ((IErrorDispatcher) this).Handler.OnError(error);
    }

    IEnumerator ISetupComponent.Setup()
    {
        _matchMakingClient.Start();
        while(!_matched)
        {
            yield return null;
        }
    }

    void DispatchStopEvent(bool successful)
    {
        if(_listener != null)
        {
            _listener.OnCancelled(successful);
        }
    }
}