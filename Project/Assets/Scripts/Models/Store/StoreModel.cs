﻿using System;
using System.Collections.Generic;
using SocialPoint.Purchase;
using SocialPoint.Base;
using SocialPoint.Utils;

public class StoreModel : IStoreProductSource, IDisposable
{
    public IDictionary<string, IReward> PurchaseRewards = new Dictionary<string, IReward>();

    public event Action<StoreModel> Moved;

    public string[] ProductIds
    {
        get
        {
            if(PurchaseRewards == null)
            {
                return null;
            }
            var value = new string[PurchaseRewards.Keys.Count];
            PurchaseRewards.Keys.CopyTo(value, 0);
            return value;
        }
    }

    IGamePurchaseStore _purchaseStore;
    PlayerModel _playerModel;

    public void Init(IGamePurchaseStore purchaseStore, PlayerModel playerModel)
    {
        _purchaseStore = purchaseStore;
        _playerModel = playerModel;

        if(_purchaseStore != null)
        {
            _purchaseStore.RegisterPurchaseCompletedDelegate(OnPurchaseCompleted);

            //Each game can set the settings to its liking, it can depend on data sent by backend
            _purchaseStore.Setup(PlatformPuchaseSettings.GetDebugSettings());
        }
    }

    public void Dispose()
    {
        if(_purchaseStore != null)
        {
            _purchaseStore.UnregisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
    }

    public void Move(StoreModel other)
    {
        PurchaseRewards = other.PurchaseRewards;

        other.PurchaseRewards = null;
        other.Dispose();

        if(Moved != null)
        {
            Moved(this);
        }
    }

    void OnProductsUpdated(LoadProductsState state, Error error)
    {
        //handle and alert view
    }

    PurchaseGameInfo OnPurchaseCompleted(Receipt receipt, PurchaseResponseType response)
    {
        //this is called from SocialPointPurchaseStore in order to aply the changes on the PlayerModel, then return a PurchaseGameInfo for analytics
        switch(response)
        {
        case PurchaseResponseType.Complete:
            IReward reward;
            if(PurchaseRewards.TryGetValue(receipt.ProductId, out reward))
            {
                UnityEngine.Debug.Log("purchase validation was ok " + receipt.ProductId);
                reward.Obtain(_playerModel);
            }
            break;
        case PurchaseResponseType.Duplicated:
        case PurchaseResponseType.Error:
        default:
            //TODO: display error? say couldn't connect to the store?
            break;
        }
        
        //TODO:fullFill this
        var purchaseGameInfo = new PurchaseGameInfo();
        return purchaseGameInfo;
    }

    public override string ToString()
    {
        return string.Format("[StoreModel: PurchaseRewards={0}]", StringUtils.Join(PurchaseRewards));
    }
}

