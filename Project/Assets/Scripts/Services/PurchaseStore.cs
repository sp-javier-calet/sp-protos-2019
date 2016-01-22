using Zenject;
using SocialPoint.ServerEvents;
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

    //String must match the binding in PurchaseInstaller
    [InjectOptional("purchase_store_product_ids")]
    string[] injectStoreProductIds
    {
        set
        {
            _storeProductIds = value;
        }
    }

    public PurchaseStore(IHttpClient httpClient, ICommandQueue commandQueue) : base(httpClient, commandQueue)
    {
    }
}