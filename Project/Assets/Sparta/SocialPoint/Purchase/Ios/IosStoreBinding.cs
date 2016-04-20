using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public class IosStoreBinding
    {
        [DllImport("__Internal")]
        private static extern void SPStore_Init(string listenerObjectName);

        public static void Init(string listenerObjectName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_Init(listenerObjectName);
        }

        [DllImport("__Internal")]
        private static extern void SPStore_SetApplicationUsername(string applicationUserName);

        // iOS 7+ only. This is used to help the store detect irregular activity.
        // The recommended implementation is to use a one-way hash of the user's account name to calculate the value for this property.
        public static void SetApplicationUsername(string applicationUserName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_SetApplicationUsername(applicationUserName);
        }

        [DllImport("__Internal")]
        private static extern void SPStore_SendTransactionUpdateEvents(bool shouldSend);

        // By default, the transactionUpdatedEvent will not be called to avoid excessive string allocations. If you pass true to this method it will be called.
        public static void SetShouldSendTransactionUpdateEvents(bool shouldSend)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_SendTransactionUpdateEvents(shouldSend);
        }


        [DllImport("__Internal")]
        private static extern void SPStore_EnableHighDetailLogs(bool shouldEnable);

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_EnableHighDetailLogs(shouldEnable);
        }


        [DllImport("__Internal")]
        private static extern void SPStore_RequestProductData(string productIdentifiers);

        // Accepts an array of product identifiers. All of the products you have for sale should be requested in one call.
        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_RequestProductData(string.Join(",", productIdentifiers));
        }


        [DllImport("__Internal")]
        private static extern void SPStore_PurchaseProduct(string productIdentifier);

        // Purchases the given product and quantity
        public static void PurchaseProduct(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_PurchaseProduct(productIdentifier);
        }

        [DllImport("__Internal")]
        private static extern void SPStore_ForceFinishPendingTransactions();

        // Force finishes any and all pending transactions including those being tracked and any random transactions in Apple's queue
        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_ForceFinishPendingTransactions();
        }


        [DllImport("__Internal")]
        private static extern void SPStore_FinishPendingTransaction(string transactionIdentifier);

        // Finishes the pending transaction identified by the transactionIdentifier
        public static void FinishPendingTransaction(string transactionIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPStore_FinishPendingTransaction(transactionIdentifier);
        }
    }
}
#endif
