using System;
using System.Collections.Generic;
using SocialPoint.Purchase;
using SocialPoint.Base;
using SocialPoint.Utils;
using Zenject;

public class StoreModel : IDisposable
{
    public IDictionary<string, StoreCategory> Categories = new Dictionary<string, StoreCategory>();
    public IDictionary<string, IReward> PurchaseRewards = new Dictionary<string, IReward>();

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

    public event Action<StoreItem> PurchaseSuccess;
    public event Action<Error> PurchaseError;

    IGamePurchaseStore _purchaseStore;

    public StoreModel(IGamePurchaseStore purchaseStore = null)
    {
        _purchaseStore = purchaseStore;
        if(_purchaseStore != null)
        {
            _purchaseStore.RegisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
    }

    public void Dispose()
    {
        _purchaseStore.UnregisterPurchaseCompletedDelegate(OnPurchaseCompleted);
    }

    public void Assign(StoreModel other)
    {
        Categories = other.Categories;
        PurchaseRewards = other.PurchaseRewards;
        if(_purchaseStore != null)
        {
            _purchaseStore.UnregisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
        _purchaseStore = other._purchaseStore;
        if(_purchaseStore != null)
        {
            _purchaseStore.RegisterPurchaseCompletedDelegate(OnPurchaseCompleted);
        }
    }

    public void OnPurchase(StoreItem storeItem)
    {        
        UnityEngine.Debug.Log("buying " + storeItem.Name);
        storeItem.Purchase((Error error) => {
            if(!Error.IsNullOrEmpty(error))
            {
                PurchaseError(error);
            }
            else
            {
                PurchaseSuccess(storeItem);
            }
        });
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
        return string.Format("[StoreModel: Categories={0}, PurchaseRewards={1}]", 
            StringUtils.Join(Categories), StringUtils.Join(PurchaseRewards));
    }
}

