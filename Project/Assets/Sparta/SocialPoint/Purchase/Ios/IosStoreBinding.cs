﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public class IosStoreBinding
    {
        [DllImport("__Internal")]
        private static extern void SPUnityStore_Init(string listenerObjectName);

        public static void Init(string listenerObjectName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_Init(listenerObjectName);
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_SetApplicationUsername(string applicationUserName);

        // iOS 7+ only. This is used to help the store detect irregular activity.
        // The recommended implementation is to use a one-way hash of the user's account name to calculate the value for this property.
        public static void SetApplicationUsername(string applicationUserName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_SetApplicationUsername(applicationUserName);
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_SetUseAppUsername(bool shouldUseAppUsername);

        // By default the application username is not set as data for a payment, but this is requested by Apple and we should set it if possible
        public static void SetUseAppUsername(bool shouldUseAppUsername)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_SetUseAppUsername(shouldUseAppUsername);
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_SetUseAppReceipt(bool shouldUseAppReceipt);

        // By default the Transaction receipt is used, but is deprecated. Set this property to true to use the App receipt if our game/backend supports it
        public static void SetUseAppReceipt(bool shouldUseAppReceipt)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_SetUseAppReceipt(shouldUseAppReceipt);
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_SendTransactionUpdateEvents(bool shouldSend);

        // By default, the transactionUpdatedEvent will not be called to avoid excessive string allocations. If you pass true to this method it will be called.
        public static void SetShouldSendTransactionUpdateEvents(bool shouldSend)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_SendTransactionUpdateEvents(shouldSend);
            }
        }


        [DllImport("__Internal")]
        private static extern void SPUnityStore_EnableHighDetailLogs(bool shouldEnable);

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_EnableHighDetailLogs(shouldEnable);
            }
        }


        [DllImport("__Internal")]
        private static extern void SPUnityStore_RequestProductData(string productIdentifiers);

        // Accepts an array of product identifiers. All of the products you have for sale should be requested in one call.
        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_RequestProductData(string.Join(",", productIdentifiers));
            }
        }


        [DllImport("__Internal")]
        private static extern void SPUnityStore_PurchaseProduct(string productIdentifier);

        // Purchases the given product and quantity
        public static void PurchaseProduct(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_PurchaseProduct(productIdentifier);
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_ForceUpdatePendingTransactions();

        // Force update any and all pending transactions to check their current states
        public static void ForceUpdatePendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_ForceUpdatePendingTransactions();
            }
        }

        [DllImport("__Internal")]
        private static extern void SPUnityStore_ForceFinishPendingTransactions();

        // Force finishes any and all pending transactions including those being tracked and any random transactions in Apple's queue
        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_ForceFinishPendingTransactions();
            }
        }


        [DllImport("__Internal")]
        private static extern void SPUnityStore_FinishPendingTransaction(string transactionIdentifier);

        // Finishes the pending transaction identified by the transactionIdentifier
        public static void FinishPendingTransaction(string transactionIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                SPUnityStore_FinishPendingTransaction(transactionIdentifier);
            }
        }
    }
}
#endif
