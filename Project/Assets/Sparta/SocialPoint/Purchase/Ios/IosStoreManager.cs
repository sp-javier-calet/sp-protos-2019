using System;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

#if UNITY_IOS
namespace SocialPoint.Purchase
{
    public class IosStoreManager : MonoBehaviour
    {
        // Fired when the product list your required returns. Automatically serializes the productString into IosStoreProduct's.
        public static event Action<List<IosStoreProduct>> ProductListReceivedEvent;

        // Fired when requesting product data fails
        public static event Action<Error> ProductListRequestFailedEvent;

        // Fired anytime Apple updates a transaction if you called setShouldSendTransactionUpdateEvents with true. Check the transaction.transactionState to
        // know what state the transaction is currently in.
        public static event Action<IosStoreTransaction> TransactionUpdatedEvent;

        // Fired when a product is successfully paid for. The event will provide a IosStoreTransaction object that holds the productIdentifer and receipt of the purchased product.
        public static event Action<IosStoreTransaction> PurchaseSuccessfulEvent;

        // Fired when a product purchase fails
        public static event Action<Error> PurchaseFailedEvent;

        // Fired when a product purchase is cancelled by the user or system
        public static event Action<Error> PurchaseCancelledEvent;

        const string instanceName = "IosStoreManager";

        static IosStoreManager()
        {
            var instance = new GameObject(instanceName);
            instance.AddComponent<IosStoreManager>();
            DontDestroyOnLoad(instance);
            IosStoreBinding.Init(instanceName);
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
            Debug.Log(logMsg);
        }
    }
}
#endif
