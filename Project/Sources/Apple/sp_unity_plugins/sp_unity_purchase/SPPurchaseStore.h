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
    EXPORT_API void SPStore_Init(const char* listenerObjectName);
    EXPORT_API void SPStore_SetApplicationUsername(const char* applicationUserName);
    EXPORT_API void SPStore_SendTransactionUpdateEvents(bool shouldSend);
    EXPORT_API void SPStore_EnableHighDetailLogs(bool shouldEnable);
    EXPORT_API void SPStore_RequestProductData(const char* productIdentifiers);
    EXPORT_API void SPStore_PurchaseProduct(const char* productIdentifier);
    EXPORT_API void SPStore_ForceFinishPendingTransactions();
    EXPORT_API void SPStore_FinishPendingTransaction(const char* transactionIdentifier);
}
    
#endif /* SPPurchaseStore_h */
