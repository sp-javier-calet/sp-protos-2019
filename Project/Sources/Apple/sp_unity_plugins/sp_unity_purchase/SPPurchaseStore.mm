#include "SPPurchaseStore.h"
#include "SPPurchaseNativeServices.h"
#import <string>
#import <vector>
#import <sstream>

/* BRIDGE */

class PlatformPurchaseCenterBridge
{
    // Native object instance
    SPPurchaseNativeServices* _purchaseServices;

  public:
    PlatformPurchaseCenterBridge()
    : _purchaseServices(nullptr)
    {
        _purchaseServices = [[SPPurchaseNativeServices alloc] init];
    }

    /* Setters */

    void setApplicationUsername(const char* userIdentifier)
    {
        [_purchaseServices setAppUsername:userIdentifier];
    }

    void sendTransactionUpdateEvents(bool shouldSend)
    {
        [_purchaseServices sendTransactionUpdateEvents:shouldSend];
    }

    void enableHighDetailLogs(bool shouldEnable)
    {
        [_purchaseServices enableHighDetailLogs:shouldEnable];
    }

    /* Product Functions */

    void downloadProducts(const std::string& productList)
    {
        if([_purchaseServices isInitializingStoreProductsIds])
        {
            return;// Already initializing products
        }

        NSMutableSet* productIdentifiers = [[NSMutableSet alloc] init];

        std::vector<std::string> products = PlatformPurchaseCenterBridge::split(productList, ',');
        for(std::string& productIdentifier : products)
        {
            NSString* productId = [NSString stringWithCString:productIdentifier.c_str() encoding:NSUTF8StringEncoding];
            [productIdentifiers addObject:productId];
        }

        [_purchaseServices startProductsRequest:productIdentifiers];
    }

    void purchaseProduct(const char* productIdentifier)
    {
        [_purchaseServices purchaseProduct:productIdentifier];
    }

    /* Transaction Functions */

    void forceUpdatePendingTransactions()
    {
        return [_purchaseServices forceUpdatePendingTransactions];
    }

    void finishPendingTransaction(const char* productIdentifier)
    {
        return [_purchaseServices finishPendingTransaction:productIdentifier];
    }

    void forceFinishPendingTransactions()
    {
        return [_purchaseServices forceFinishPendingTransactions];
    }

  private:
    /* Utility Functions */

    static std::vector<std::string> split(const std::string& s, char delim)
    {
        std::vector<std::string> elems;
        split(s, delim, elems);
        return elems;
    }

    static std::vector<std::string>& split(const std::string& s, char delim, std::vector<std::string>& elems)
    {
        std::stringstream ss(s);
        std::string item;
        while(std::getline(ss, item, delim))
        {
            elems.push_back(item);
        }
        return elems;
    }
};

// Bridge Instance
PlatformPurchaseCenterBridge* purchaseBridge = nullptr;

/* EXPORT FUNCTIONS */

EXPORT_API void SPUnityStore_Init()
{
    purchaseBridge = new PlatformPurchaseCenterBridge();
}

EXPORT_API void SPUnityStore_SetApplicationUsername(const char* applicationUserName)
{
    if(purchaseBridge)
    {
        purchaseBridge->setApplicationUsername(applicationUserName);
    }
}

EXPORT_API void SPUnityStore_SendTransactionUpdateEvents(bool shouldSend)
{
    if(purchaseBridge)
    {
        purchaseBridge->sendTransactionUpdateEvents(shouldSend);
    }
}

EXPORT_API void SPUnityStore_EnableHighDetailLogs(bool shouldEnable)
{
    if(purchaseBridge)
    {
        purchaseBridge->enableHighDetailLogs(shouldEnable);
    }
}

EXPORT_API void SPUnityStore_RequestProductData(const char* productIdentifiers)
{
    if(purchaseBridge)
    {
        purchaseBridge->downloadProducts(productIdentifiers);
    }
}

EXPORT_API void SPUnityStore_PurchaseProduct(const char* productIdentifier)
{
    if(purchaseBridge)
    {
        purchaseBridge->purchaseProduct(productIdentifier);
    }
}

EXPORT_API void SPUnityStore_ForceUpdatePendingTransactions()
{
    if(purchaseBridge)
    {
        purchaseBridge->forceUpdatePendingTransactions();
    }
}

EXPORT_API void SPUnityStore_ForceFinishPendingTransactions()
{
    if(purchaseBridge)
    {
        purchaseBridge->forceFinishPendingTransactions();
    }
}

EXPORT_API void SPUnityStore_FinishPendingTransaction(const char* transactionIdentifier)
{
    if(purchaseBridge)
    {
        purchaseBridge->finishPendingTransaction(transactionIdentifier);
    }
}
