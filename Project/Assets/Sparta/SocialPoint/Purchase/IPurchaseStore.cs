using System;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;

namespace SocialPoint.Purchase
{
    public enum PurchaseResponseType
    {
        Error = -1,
        Complete = 0,
        Duplicated = 1
    }

    public enum LoadProductsState
    {
        Success,
        Error
    }

    public enum PurchaseState
    {
        PurchaseStarted,
        ValidateSuccess,
        ValidateFailed,
        ValidateFailedMissingNetwork,
        PurchaseFailed,
        PurchaseCanceled,
        PurchaseFinished,
        PurchaseConsumed,
        AlreadyBeingPurchased,
        RemovedTransaction
    }

    public delegate void ProductsUpdatedDelegate(LoadProductsState state, Error error = null);

    public delegate void PurchaseUpdatedDelegate(PurchaseState state, string productId);

    public delegate void ValidatePurchaseResponseDelegate(PurchaseResponseType response);

    public delegate void ValidatePurchaseDelegate(Receipt receipt, ValidatePurchaseResponseDelegate response);

    public interface IPurchaseStore : IDisposable
    {
        Product[] ProductList{ get; }

        bool HasProductsLoaded{ get; }

        void Setup(AttrDic settings);

        void LoadProducts(string[] productIds);

        bool Purchase(string productId);

        event ProductsUpdatedDelegate ProductsUpdated;
        event PurchaseUpdatedDelegate PurchaseUpdated;

        ValidatePurchaseDelegate ValidatePurchase{ set; }

        ILoginData LoginData { get; set; }

        void ForceFinishPendingTransactions();

        void PurchaseStateChanged(PurchaseState state, string productID);
    }
}
