using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public sealed class AndroidStoreManager
    {
        //Successfull Init callback. Billing is supported on current platform
        public event Action BillingSupportedEvent;

        //Failed Init callback. Billing is not supported on current platform
        public event Action<Error> BillingNotSupportedEvent;

        //Successful QueryInventory callback. Purchase history and store listings are returned/
        public event Action<List<AndroidStoreProduct>> QueryInventorySucceededEvent;

        //Failed QueryInventory callback.
        public event Action<Error> QueryInventoryFailedEvent;

        //Successful purchase callback. Fired after purchase of a product or a subscription
        public event Action<AndroidStoreTransaction> PurchaseSucceededEvent;

        //Failed purchase callback
        public event Action<Error> PurchaseFailedEvent;

        //Canceled purchase callback
        public event Action<Error> PurchaseCancelledEvent;

        //Successful consume attempt callback
        public event Action<AndroidStoreTransaction> ConsumePurchaseSucceededEvent;

        //Failed consume attempt callback
        public event Action<Error> ConsumePurchaseFailedEvent;

        NativeCallsHandler _handler;

        public AndroidStoreManager(NativeCallsHandler handler)
        {
            _handler = handler;

            _handler.RegisterListener("OnBillingNotSupported", OnBillingNotSupported);
            _handler.RegisterListener("OnBillingSupported", OnBillingSupported);
            _handler.RegisterListener("OnQueryInventoryFailed", OnQueryInventoryFailed);
            _handler.RegisterListener("OnQueryInventorySucceeded", OnQueryInventorySucceeded);
            _handler.RegisterListener("OnPurchaseFailed", OnPurchaseFailed);
            _handler.RegisterListener("OnPurchaseSucceeded", OnPurchaseSucceeded);
            _handler.RegisterListener("OnPurchaseCancelled", OnPurchaseCancelled);
            _handler.RegisterListener("OnConsumePurchaseFailed", OnConsumePurchaseFailed);
            _handler.RegisterListener("OnConsumePurchaseSucceeded", OnConsumePurchaseSucceeded);

            AndroidStoreBinding.Init();
        }

        void OnBillingSupported()
        {
            if(BillingSupportedEvent != null)
            {
                BillingSupportedEvent();
            }
        }

        void OnBillingNotSupported(string error)
        {
            if(BillingNotSupportedEvent != null)
            {
                BillingNotSupportedEvent(new Error(error));
            }
        }

        void OnQueryInventorySucceeded(string json)
        {
            if(QueryInventorySucceededEvent != null)
            {
                QueryInventorySucceededEvent(AndroidStoreProduct.ProductsFromJson(json));
            }
        }

        void OnQueryInventoryFailed(string error)
        {
            if(QueryInventoryFailedEvent != null)
            {
                QueryInventoryFailedEvent(new Error(error));
            }
        }

        void OnPurchaseSucceeded(string json)
        {
            if(PurchaseSucceededEvent != null)
            {
                PurchaseSucceededEvent(AndroidStoreTransaction.TransactionFromJson(json));
            }
        }

        void OnPurchaseFailed(string message)
        {
            if(PurchaseFailedEvent != null)
            {
                PurchaseFailedEvent(new Error(message));
            }
        }

        void OnPurchaseCancelled(string message)
        {
            if(PurchaseFailedEvent != null)
            {
                PurchaseCancelledEvent(new Error(message));
            }
        }

        void OnConsumePurchaseSucceeded(string json)
        {
            if(ConsumePurchaseSucceededEvent != null)
            {
                ConsumePurchaseSucceededEvent(AndroidStoreTransaction.TransactionFromJson(json));
            }
        }

        void OnConsumePurchaseFailed(string error)
        {
            if(ConsumePurchaseFailedEvent != null)
            {
                ConsumePurchaseFailedEvent(new Error(error));
            }
        }

        //May be called from Android plugin if needed to debug in Unity side
        public void StoreDebugLog(string logMsg)
        {
            Log.i(logMsg);
        }
    }
}
#endif
