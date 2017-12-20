using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public sealed class PurchaseGameInfo
    {
        public string OfferName;
        public string ResourceName;
        public int ResourceAmount;
        public AttrDic AdditionalData;
    }

    public delegate void ProductReadyDelegate(string productId);
    public delegate PurchaseGameInfo PurchaseCompletedDelegate(Receipt receipt, PurchaseResponseType response);

    public interface IGamePurchaseStore
    {
        string Currency { get; set; }

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

        void RegisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate);

        void RegisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate, float timeout);

        void UnregisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate);

        void UnregisterProductReadyDelegate(ProductReadyDelegate pDelegate);
    }

}
