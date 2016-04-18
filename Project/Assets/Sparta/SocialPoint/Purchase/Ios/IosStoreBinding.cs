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
        private static extern bool SPCanMakePayments();

        public static bool CanMakePayments()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                return SPCanMakePayments();
            return false;
        }


        [DllImport("__Internal")]
        private static extern void SPSetApplicationUsername(string applicationUserName);

        // iOS 7+ only. This is used to help the store detect irregular activity.
        // The recommended implementation is to use a one-way hash of the user's account name to calculate the value for this property.
        public static void SetApplicationUsername(string applicationUserName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPSetApplicationUsername(applicationUserName);
        }


        [DllImport("__Internal")]
        private static extern string SPGetAppStoreReceiptUrl();

        // iOS 7 only. Returns the location of the App Store receipt file. If called on an older iOS version it returns null.
        public static string GetAppStoreReceiptLocation()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                return SPGetAppStoreReceiptUrl();

            return null;
        }


        [DllImport("__Internal")]
        private static extern void SPSendTransactionUpdateEvents(bool sendTransactionUpdateEvents);

        // By default, the transactionUpdatedEvent will not be called to avoid excessive string allocations. If you pass true to this method it will be called.
        public static void SetShouldSendTransactionUpdateEvents(bool sendTransactionUpdateEvents)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPSendTransactionUpdateEvents(sendTransactionUpdateEvents);
        }


        [DllImport("__Internal")]
        private static extern void SPEnableHighDetailLogs(bool shouldEnable);

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPEnableHighDetailLogs(shouldEnable);
        }


        [DllImport("__Internal")]
        private static extern void SPRequestProductData(string productIdentifier);

        // Accepts an array of product identifiers. All of the products you have for sale should be requested in one call.
        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPRequestProductData(string.Join(",", productIdentifiers));
        }


        [DllImport("__Internal")]
        private static extern void SPPurchaseProduct(string productIdentifier, int quantity);

        // Purchases the given product and quantity
        public static void PurchaseProduct(string productIdentifier, int quantity)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPPurchaseProduct(productIdentifier, quantity);
        }


        [DllImport("__Internal")]
        private static extern void SPFinishPendingTransactions();

        // Finishes any pending transactions that were being tracked
        public static void FinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPFinishPendingTransactions();
        }


        [DllImport("__Internal")]
        private static extern void SPForceFinishPendingTransactions();

        // Force finishes any and all pending transactions including those being tracked and any random transactions in Apple's queue
        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPFinishPendingTransactions();
        }


        [DllImport("__Internal")]
        private static extern void SPFinishPendingTransaction(string transactionIdentifier);

        // Finishes the pending transaction identified by the transactionIdentifier
        public static void FinishPendingTransaction(string transactionIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPFinishPendingTransaction(transactionIdentifier);
        }


        [DllImport("__Internal")]
        private static extern void SPPauseDownloads();

        // Pauses any pending downloads
        public static void PauseDownloads()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPPauseDownloads();
        }


        [DllImport("__Internal")]
        private static extern void SPResumeDownloads();

        // Resumes any pending paused downloads
        public static void ResumeDownloads()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPResumeDownloads();
        }


        [DllImport("__Internal")]
        private static extern void SPCancelDownloads();

        // Cancels any pending downloads
        public static void CancelDownloads()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPCancelDownloads();
        }


        [DllImport("__Internal")]
        private static extern void SPRestoreCompletedTransactions();

        // Restores all previous transactions.  This is used when a user gets a new device and they need to restore their old purchases.
        // DO NOT call this on every launch.  It will prompt the user for their password. Each transaction that is restored will have the normal
        // purchaseSuccessfulEvent fire for when restoration is complete.
        public static void RestoreCompletedTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPRestoreCompletedTransactions();
        }


        [DllImport("__Internal")]
        private static extern string SPGetAllSavedTransactions();

        // Returns a list of all the transactions that occured on this device.  They are stored in the Document directory.
        public static List<IosStoreTransaction> GetAllSavedTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // Grab the transactions and parse them out
                var json = SPGetAllSavedTransactions();
                return IosStoreTransaction.TransactionsFromJson(json);
            }

            return new List<IosStoreTransaction>();
        }


        [DllImport("__Internal")]
        private static extern void SPDisplayStoreWithProductId(string productId, string affiliateToken);

        // iOS 6+ only! Displays the App Store with the given productId in app. The affiliateToken parameter will only work on iOS 8+.
        public static void DisplayStoreWithProductId(string productId, string affiliateToken = null)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SPDisplayStoreWithProductId(productId, affiliateToken);
        }

    }
}
#endif
