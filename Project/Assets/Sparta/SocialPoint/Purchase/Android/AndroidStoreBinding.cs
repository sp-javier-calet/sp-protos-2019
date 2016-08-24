using UnityEngine;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public static class AndroidStoreBinding
    {
        const string FullClassName = "es.socialpoint.sparta.purchase.SPPurchaseStore";

        const string FunctionInit = "Init";
        const string FunctionInitWithLogs = "InitWithLogs";
        const string FunctionEnableHighDetailLogs = "EnableHighDetailLogs";
        const string FunctionRequestProductData = "RequestProductData";
        const string FunctionPurchaseProduct = "PurchaseProduct";
        const string FunctionFinishPendingTransaction = "FinishPendingTransaction";
        const string FunctionForceFinishPendingTransactions = "ForceFinishPendingTransactions";
        const string FunctionUnbind = "Unbind";

        static AndroidJavaClass _notifClass;

        public static void Init(bool highDetailLogsEnabled = false)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass = new AndroidJavaClass(FullClassName);
                if(highDetailLogsEnabled)
                {
                    _notifClass.CallStatic(FunctionInitWithLogs);
                }
                else
                {
                    _notifClass.CallStatic(FunctionInit);
                }
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
