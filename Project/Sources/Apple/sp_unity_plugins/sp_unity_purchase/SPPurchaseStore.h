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

    EXPORT_API bool SPCanMakePayments();
    EXPORT_API void SPSetApplicationUsername(const char* applicationUserName);
    EXPORT_API const char* SPGetAppStoreReceiptUrl();
    EXPORT_API void SPSendTransactionUpdateEvents(bool sendTransactionUpdateEvents);
    EXPORT_API void SPEnableHighDetailLogs(bool shouldEnable);
    EXPORT_API void SPRequestProductData(const char* productIdentifier);
    EXPORT_API void SPPurchaseProduct(const char* productIdentifier, int quantity);
    EXPORT_API void SPFinishPendingTransactions();
    EXPORT_API void SPForceFinishPendingTransactions();
    EXPORT_API void SPFinishPendingTransaction(const char* transactionIdentifier);
    EXPORT_API void SPPauseDownloads();
    EXPORT_API void SPResumeDownloads();
    EXPORT_API void SPCancelDownloads();
    EXPORT_API void SPRestoreCompletedTransactions();
    EXPORT_API const char* SPGetAllSavedTransactions();
    EXPORT_API void SPDisplayStoreWithProductId(const char* productId, const char* affiliateToken);
}
    
#endif /* SPPurchaseStore_h */
