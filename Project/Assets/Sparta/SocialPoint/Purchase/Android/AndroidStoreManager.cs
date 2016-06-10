﻿using System;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public class AndroidStoreManager : MonoBehaviour
    {
        //Successfull Init callback. Billing is supported on current platform
        public static event Action BillingSupportedEvent;

        //Failed Init callback. Billing is not supported on current platform
        public static event Action<Error> BillingNotSupportedEvent;

        //Successful QueryInventory callback. Purchase history and store listings are returned/
        public static event Action<List<AndroidStoreProduct>> QueryInventorySucceededEvent;

        //Failed QueryInventory callback.
        public static event Action<Error> QueryInventoryFailedEvent;

        //Successful purchase callback. Fired after purchase of a product or a subscription
        public static event Action<AndroidStoreTransaction> PurchaseSucceededEvent;

        //Failed purchase callback
        public static event Action<Error> PurchaseFailedEvent;

        //Canceled purchase callback
        public static event Action<Error> PurchaseCancelledEvent;

        //Successful consume attempt callback
        public static event Action<AndroidStoreTransaction> ConsumePurchaseSucceededEvent;

        //Failed consume attempt callback
        public static event Action<Error> ConsumePurchaseFailedEvent;

        const string instanceName = "AndroidStoreManager";

        static AndroidStoreManager()
        {
            var instance = new GameObject(instanceName);
            instance.AddComponent<AndroidStoreManager>();
            DontDestroyOnLoad(instance);

            AndroidStoreBinding.Init(instanceName);
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
            Debug.Log(logMsg);
        }
    }
}
#endif
