using System;
using System.Collections.Generic;
using SocialPoint.Purchase;
using SocialPoint.Base;
using SocialPoint.Utils;
using Zenject;

public class StoreModel : IStoreProductSource, IDisposable
{
    public IDictionary<string, IReward> PurchaseRewards = new Dictionary<string, IReward>();

    public event Action<StoreModel> Assigned;

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

    public void Init(IGamePurchaseStore purchaseStore)
    {
        _purchaseStore = purchaseStore;
        if(_purchaseStore != null)
        {
            _purchaseStore.RegisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
    }

    public void Dispose()
    {
        if(_purchaseStore != null)
        {
            _purchaseStore.UnregisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
    }

    public void Assign(StoreModel other)
    {
        PurchaseRewards = other.PurchaseRewards;

        if(Assigned != null)
        {
            Assigned(this);
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
            if(PurchaseRewards[receipt.ProductId] != null)
            {
                UnityEngine.Debug.Log("purchase validation was ok " + receipt.ProductId);
                PurchaseRewards[receipt.ProductId].Obtain();
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

