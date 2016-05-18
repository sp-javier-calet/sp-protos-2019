#ifndef SPPurchaseStore_h
#define SPPurchaseStore_h

// Which platform we are on?
#if _MSC_VER
#define UNITY_WIN 1
#else
#define UNITY_OSX 1
#endif

// Attribute to make function be exported from a plugin
#if UNITY_WIN
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API
#endif

extern "C"
{
    EXPORT_API void SPUnityStore_Init(const char* listenerObjectName);
    EXPORT_API void SPUnityStore_SetApplicationUsername(const char* applicationUserName);
    EXPORT_API void SPUnityStore_SetUseAppReceipt(bool shouldUseAppReceipt);
    EXPORT_API void SPUnityStore_SendTransactionUpdateEvents(bool shouldSend);
    EXPORT_API void SPUnityStore_EnableHighDetailLogs(bool shouldEnable);
    EXPORT_API void SPUnityStore_RequestProductData(const char* productIdentifiers);
    EXPORT_API void SPUnityStore_PurchaseProduct(const char* productIdentifier);
    EXPORT_API void SPUnityStore_ForceUpdatePendingTransactions();
    EXPORT_API void SPUnityStore_ForceFinishPendingTransactions();
    EXPORT_API void SPUnityStore_FinishPendingTransaction(const char* transactionIdentifier);
}
    
#endif /* SPPurchaseStore_h */
