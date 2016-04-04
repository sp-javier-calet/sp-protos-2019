
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;
using SocialPoint.Purchase;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using UnityEngine;

class PurchaseStore : SocialPointPurchaseStore
{
    public PurchaseStore(IHttpClient httpClient, ICommandQueue commandQueue, StoreModel store) : base(httpClient, commandQueue)
    {
        TrackEvent = ServiceLocator.Instance.Resolve<IEventTracker>().TrackSystemEvent;
        RequestSetup = ServiceLocator.Instance.Resolve<ILogin>().SetupHttpRequest;
        store.Init(this);
    }
}