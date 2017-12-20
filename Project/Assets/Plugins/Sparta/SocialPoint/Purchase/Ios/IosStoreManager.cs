using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;

#if (UNITY_IOS || UNITY_TVOS)
namespace SocialPoint.Purchase
{
    public sealed class IosStoreManager
    {
        // Fired when the product list your required returns. Automatically serializes the productString into IosStoreProduct's.
        public event Action<List<IosStoreProduct>> ProductListReceivedEvent;

        // Fired when requesting product data fails
        public event Action<Error> ProductListRequestFailedEvent;

        // Fired anytime Apple updates a transaction if you called setShouldSendTransactionUpdateEvents with true. Check the transaction.transactionState to
        // know what state the transaction is currently in.
        public event Action<IosStoreTransaction> TransactionUpdatedEvent;

        // Fired when a product is successfully paid for. The event will provide a IosStoreTransaction object that holds the productIdentifer and receipt of the purchased product.
        public event Action<IosStoreTransaction> PurchaseSuccessfulEvent;

        // Fired when a product purchase fails
        public event Action<Error> PurchaseFailedEvent;

        // Fired when a product purchase is cancelled by the user or system
        public event Action<Error> PurchaseCancelledEvent;

        NativeCallsHandler _handler;

        public IosStoreManager(NativeCallsHandler handler)
        {
            _handler = handler;
            _handler.RegisterListener("StoreDebugLog", StoreDebugLog);
            _handler.RegisterListener("ProductsReceived", ProductsReceived);
            _handler.RegisterListener("ProductsRequestDidFail", ProductsRequestDidFail);
            _handler.RegisterListener("ProductPurchased", ProductPurchased);
            _handler.RegisterListener("ProductPurchaseCancelled", ProductPurchaseCancelled);
            _handler.RegisterListener("ProductPurchaseFailed", ProductPurchaseFailed);
            _handler.RegisterListener("TransactionUpdated", TransactionUpdated);

            IosStoreBinding.Init();
        }


        public void TransactionUpdated(string json)
        {
            if(TransactionUpdatedEvent != null)
            {
                TransactionUpdatedEvent(IosStoreTransaction.TransactionFromJson(json));
            }
        }


        public void ProductPurchased(string json)
        {
            if(PurchaseSuccessfulEvent != null)
            {
                PurchaseSuccessfulEvent(IosStoreTransaction.TransactionFromJson(json));
            }
        }


        public void ProductPurchaseFailed(string error)
        {
            if(PurchaseFailedEvent != null)
            {
                PurchaseFailedEvent(new Error(error));
            }
        }


        public void ProductPurchaseCancelled(string error)
        {
            if(PurchaseCancelledEvent != null)
            {
                PurchaseCancelledEvent(new Error(error));
            }
        }


        public void ProductsReceived(string json)
        {
            if(ProductListReceivedEvent != null)
            {
                ProductListReceivedEvent(IosStoreProduct.ProductsFromJson(json));
            }
        }


        public void ProductsRequestDidFail(string error)
        {
            if(ProductListRequestFailedEvent != null)
            {
                ProductListRequestFailedEvent(new Error(error));
            }
        }

        //May be called from Xcode plugin if needed to debug in Unity side
        public void StoreDebugLog(string logMsg)
        {
            Log.d(logMsg);
        }
    }
}
#endif
