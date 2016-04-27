﻿using Zenject;
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
            GetUserId = () => value.UserId;
        }
    }

    public PurchaseStore(IHttpClient httpClient, ICommandQueue commandQueue, StoreModel store) : base(httpClient, commandQueue)
    {
        store.Init(this);
    }
}