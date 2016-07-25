using System.Runtime.InteropServices;
using UnityEngine;

#if (UNITY_IOS || UNITY_TVOS)
namespace SocialPoint.Purchase
{
    public static class IosStoreBinding
    {
        [DllImport("__Internal")]
        static extern void SPUnityStore_Init();

        public static void Init()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_Init();
            }
        }

        [DllImport("__Internal")]
        static extern void SPUnityStore_SetApplicationUsername(string applicationUserName);

        // IMPORTANT: This is mandatory!
        // This is used to help the store detect irregular activity. Is requested by Apple for future featurings. Works in iOS 7+.
        // The recommended implementation is to use a one-way hash of the user's account name to calculate the value for this property.
        public static void SetApplicationUsername(string applicationUserName)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_SetApplicationUsername(applicationUserName);
            }
        }

        [DllImport("__Internal")]
        static extern void SPUnityStore_SendTransactionUpdateEvents(bool shouldSend);

        // By default, the transactionUpdatedEvent will not be called to avoid excessive string allocations. If you pass true to this method it will be called.
        public static void SetShouldSendTransactionUpdateEvents(bool shouldSend)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_SendTransactionUpdateEvents(shouldSend);
            }
        }


        [DllImport("__Internal")]
        static extern void SPUnityStore_EnableHighDetailLogs(bool shouldEnable);

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_EnableHighDetailLogs(shouldEnable);
            }
        }


        [DllImport("__Internal")]
        static extern void SPUnityStore_RequestProductData(string productIdentifiers);

        // Accepts an array of product identifiers. All of the products you have for sale should be requested in one call.
        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_RequestProductData(string.Join(",", productIdentifiers));
            }
        }


        [DllImport("__Internal")]
        static extern void SPUnityStore_PurchaseProduct(string productIdentifier);

        // Purchases the given product and quantity
        public static void PurchaseProduct(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_PurchaseProduct(productIdentifier);
            }
        }

        [DllImport("__Internal")]
        static extern void SPUnityStore_ForceUpdatePendingTransactions();

        // Force update any and all pending transactions to check their current states
        public static void ForceUpdatePendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_ForceUpdatePendingTransactions();
            }
        }

        [DllImport("__Internal")]
        static extern void SPUnityStore_ForceFinishPendingTransactions();

        // Force finishes any and all pending transactions including those being tracked and any random transactions in Apple's queue
        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_ForceFinishPendingTransactions();
            }
        }


        [DllImport("__Internal")]
        static extern void SPUnityStore_FinishPendingTransaction(string transactionIdentifier);

        // Finishes the pending transaction identified by the transactionIdentifier
        public static void FinishPendingTransaction(string transactionIdentifier)
        {
            if(Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.tvOS)
            {
                SPUnityStore_FinishPendingTransaction(transactionIdentifier);
            }
        }
    }
}
#endif
