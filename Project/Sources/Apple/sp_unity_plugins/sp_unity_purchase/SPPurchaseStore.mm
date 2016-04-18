#include "SPPurchaseStore.h"
#import <Foundation/Foundation.h>

EXPORT_API bool SPCanMakePayments()
{
    return false;
}

EXPORT_API void SPSetApplicationUsername(const char* applicationUserName)
{
    
}

EXPORT_API const char* SPGetAppStoreReceiptUrl()
{
    return "";
}

EXPORT_API void SPSendTransactionUpdateEvents(bool sendTransactionUpdateEvents)
{
    
}

EXPORT_API void SPEnableHighDetailLogs(bool shouldEnable)
{
    
}

EXPORT_API void SPRequestProductData(const char* productIdentifier)
{
    
}

EXPORT_API void SPPurchaseProduct(const char* productIdentifier, int quantity)
{
    
}

EXPORT_API void SPFinishPendingTransactions()
{
    
}

EXPORT_API void SPForceFinishPendingTransactions()
{
    
}

EXPORT_API void SPFinishPendingTransaction(const char* transactionIdentifier)
{
    
}

EXPORT_API void SPPauseDownloads()
{
    
}

EXPORT_API void SPResumeDownloads()
{
    
}

EXPORT_API void SPCancelDownloads()
{
    
}

EXPORT_API void SPRestoreCompletedTransactions()
{
    
}

EXPORT_API const char* SPGetAllSavedTransactions()
{
    return "";
}

EXPORT_API void SPDisplayStoreWithProductId(const char* productId, const char* affiliateToken)
{
    
}
