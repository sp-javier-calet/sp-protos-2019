using Zenject;
using SocialPoint.Events;
using SocialPoint.Purchase;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using UnityEngine;

class PurchaseStore : SocialPointPurchaseStore
{
    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackSystemEvent;
        }
    }

    [Inject]
    ILogin injectLogin
    {
        set
        {
            RequestSetup = value.SetupHttpRequest;
        }
    }

    public PurchaseStore(IHttpClient httpClient, ICommandQueue commandQueue) : base(httpClient, commandQueue)
    {
    }
}