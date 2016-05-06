using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public class AndroidStoreManager : MonoBehaviour
    {
        //Successfull Init callback. Billing is supported on current platform
        public static event Action BillingSupportedEvent;

        //Failed Init callback. Billing is not supported on current platform
        public static event Action<string> BillingNotSupportedEvent;

        //Successful QueryInventory callback. Purchase history and store listings are returned/
        public static event Action<List<AndroidStoreProduct>> QueryInventorySucceededEvent;

        //Failed QueryInventory callback.
        public static event Action<string> QueryInventoryFailedEvent;

        //Successful purchase callback. Fired after purchase of a product or a subscription
        public static event Action<AndroidStoreTransaction> PurchaseSucceededEvent;

        //Failed purchase callback
        public static event Action<string> PurchaseFailedEvent;

        //Canceled purchase callback
        public static event Action<string> PurchaseCancelledEvent;

        //Successful consume attempt callback
        public static event Action<AndroidStoreTransaction> ConsumePurchaseSucceededEvent;

        //Failed consume attempt callback
        public static event Action<string> ConsumePurchaseFailedEvent;

        //Fired when transaction was restored
        public static event Action<string> TransactionRestoredEvent;

        //Fired when transaction restoration process failed
        public static event Action<string> RestoreFailedEvent;

        //Fired when transaction restoration process succeeded
        public static event Action RestoreSucceededEvent;


        static AndroidStoreManager()
        {
            string instanceName = "AndroidStoreManager";
            GameObject instance = new GameObject(instanceName);
            instance.AddComponent<AndroidStoreManager>();
            DontDestroyOnLoad(instance);

            AndroidStoreBinding.Init(instanceName);
        }

        private void OnMapSkuFailed(string exception)
        {
            Debug.LogError("SKU mapping failed: " + exception);
        }

        private void OnBillingSupported(string empty)
        {
            if(BillingSupportedEvent != null)
            {
                BillingSupportedEvent();
            }
        }

        private void OnBillingNotSupported(string error)
        {
            if(BillingNotSupportedEvent != null)
            {
                BillingNotSupportedEvent(error);
            }
        }

        private void OnQueryInventorySucceeded(string json)
        {
            Debug.Log("*** TEST Query Inventory Succeeded: " + json);
            if(QueryInventorySucceededEvent != null)
            {
                QueryInventorySucceededEvent(AndroidStoreProduct.ProductsFromJson(json));
            }
        }

        private void OnQueryInventoryFailed(string error)
        {
            Debug.Log("*** TEST Query Inventory Failed: " + error);
            if(QueryInventoryFailedEvent != null)
            {
                QueryInventoryFailedEvent(error);
            }
        }

        private void OnPurchaseSucceeded(string json)
        {
            Debug.Log("*** TEST OnPurchaseSucceeded: " + json);
            if(PurchaseSucceededEvent != null)
            {
                PurchaseSucceededEvent(AndroidStoreTransaction.TransactionFromJson(json));
            }
        }

        private void OnPurchaseFailed(string message)
        {
            Debug.Log("*** TEST OnPurchaseFailed: " + message);
            if(PurchaseFailedEvent != null)
            {
                PurchaseFailedEvent(message);
            }
        }

        private void OnPurchaseCancelled(string message)
        {
            Debug.Log("*** TEST OnPurchaseCancelled: " + message);
            if(PurchaseFailedEvent != null)
            {
                PurchaseCancelledEvent(message);
            }
        }

        private void OnConsumePurchaseSucceeded(string json)
        {
            Debug.Log("*** TEST OnConsumePurchaseSucceeded: " + json);
            if(ConsumePurchaseSucceededEvent != null)
            {
                ConsumePurchaseSucceededEvent(AndroidStoreTransaction.TransactionFromJson(json));
            }
        }

        private void OnConsumePurchaseFailed(string error)
        {
            Debug.Log("*** TEST OnConsumePurchaseFailed: " + error);
            if(ConsumePurchaseFailedEvent != null)
            {
                ConsumePurchaseFailedEvent(error);
            }
        }

        public void OnTransactionRestored(string sku)
        {
            if(TransactionRestoredEvent != null)
            {
                TransactionRestoredEvent(sku);
            }
        }

        public void OnRestoreTransactionFailed(string error)
        {
            if(RestoreFailedEvent != null)
            {
                RestoreFailedEvent(error);
            }
        }

        public void OnRestoreTransactionSucceeded(string message)
        {
            if(RestoreSucceededEvent != null)
            {
                RestoreSucceededEvent();
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
