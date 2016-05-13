using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public class AndroidStoreBinding
    {
        private const string FullClassName = "es.socialpoint.sparta.purchase.SPPurchaseStore";

        private const string Function_Init = "Init";
        private const string Function_EnableHighDetailLogs = "EnableHighDetailLogs";
        private const string Function_RequestProductData = "RequestProductData";
        private const string Function_PurchaseProduct = "PurchaseProduct";
        private const string Function_FinishPendingTransaction = "FinishPendingTransaction";
        private const string Function_ForceFinishPendingTransactions = "ForceFinishPendingTransactions";
        private const string Function_Unbind = "Unbind";

        private static AndroidJavaClass _notifClass = null;

        public static void Init(string listenerObjectName)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    _notifClass = new AndroidJavaClass(FullClassName);
                    _notifClass.CallStatic(Function_Init, listenerObjectName);
                }
            }
        }

        // Enables/disables high detail logs
        public static void EnableHighDetailLogs(bool shouldEnable)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_EnableHighDetailLogs, shouldEnable);
            }
        }

        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_RequestProductData, string.Join(",", productIdentifiers));
            }
        }

        public static void PurchaseProduct(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_PurchaseProduct, productIdentifier);
            }
        }

        public static void FinishPendingTransaction(string productIdentifier)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_FinishPendingTransaction, productIdentifier);
            }
        }

        public static void ForceFinishPendingTransactions()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_ForceFinishPendingTransactions);
            }
        }

        public static void Unbind()
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic(Function_Unbind);
            }
        }
    }
}
#endif
