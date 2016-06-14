using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class PurchaseGameInfo
    {
        public string OfferName;
        public string ResourceName;
        public int ResourceAmount;
        public AttrDic AdditionalData;
    }

    public delegate PurchaseGameInfo PurchaseCompletedDelegate(Receipt receipt, PurchaseResponseType response);

    public interface IGamePurchaseStore
    {
        event ProductsUpdatedDelegate ProductsUpdated;
        event PurchaseUpdatedDelegate PurchaseUpdated;

        //Change desired settings. Use with PlatformPurchaseSettings
        void Setup(AttrDic settings);

        Product[] ProductList { get; }

        bool HasProductsLoaded { get; }

        void LoadProducts(string[] productIds);

        void SetProductMockList(IEnumerable<Product> productMockList);

        bool Purchase(string productId, Action<PurchaseResponseType> finished = null);

        void RegisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate);

        void UnregisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate);

        void ForceFinishPendingTransactions();
    }

}
