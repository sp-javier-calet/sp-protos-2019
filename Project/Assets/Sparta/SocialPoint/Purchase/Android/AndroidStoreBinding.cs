using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public class AndroidStoreBinding
    {
        private const string FullClassName = "es.socialpoint.sparta.purchase.SPPurchaseStore";

        private const string FunctionInit = "Init";
        private const string FunctionEnableHighDetailLogs = "EnableHighDetailLogs";
        private const string FunctionRequestProductData = "RequestProductData";
        private const string FunctionPurchaseProduct = "PurchaseProduct";
        private const string FunctionFinishPendingTransaction = "FinishPendingTransaction";
        private const string FunctionForceFinishPendingTransactions = "ForceFinishPendingTransactions";
        private const string FunctionUnbind = "Unbind";

        private static AndroidJavaClass _notifClass = null;

        public static void Init(string listenerObjectName)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass = new AndroidJavaClass(FullClassName);
                _notifClass.CallStatic(FunctionInit, listenerObjectName);
            }
        }

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionEnableHighDetailLogs, shouldEnable);
            }
        }

        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionRequestProductData, string.Join(",", productIdentifiers));
            }
        }

        public static void PurchaseProduct(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionPurchaseProduct, productIdentifier);
            }
        }

        public static void FinishPendingTransaction(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionFinishPendingTransaction, productIdentifier);
            }
        }

        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionForceFinishPendingTransactions);
            }
        }

        public static void Unbind()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(FunctionUnbind);
            }
        }
    }
}
#endif
