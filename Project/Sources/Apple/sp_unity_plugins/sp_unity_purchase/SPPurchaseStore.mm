#include "SPPurchaseStore.h"
#include "SPPurchaseNative.h"
#import <string>
#import <vector>
#import <sstream>

/* BRIDGE */

class PlatformPurchaseCenterBridge
{
    //Native object instance
    PlatformPurchaseServices* _purchaseServices;
    
public:
    PlatformPurchaseCenterBridge(const char* listenerObjectName)
    : _purchaseServices(nullptr)
    {
        _purchaseServices = [[PlatformPurchaseServices alloc] initWithUnityListener:listenerObjectName];
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
            return;//Already initializing products
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
    
    static std::vector<std::string> split(const std::string &s, char delim) {
        std::vector<std::string> elems;
        split(s, delim, elems);
        return elems;
    }
    
    static std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems) {
        std::stringstream ss(s);
        std::string item;
        while (std::getline(ss, item, delim)) {
            elems.push_back(item);
        }
        return elems;
    }
};

//Bridge Instance
PlatformPurchaseCenterBridge* purchaseBridge = nullptr;

/* EXPORT FUNCTIONS */

EXPORT_API void SPStore_Init(const char* listenerObjectName)
{
    if(purchaseBridge)
        purchaseBridge = new PlatformPurchaseCenterBridge(listenerObjectName);
}

EXPORT_API void SPStore_SetApplicationUsername(const char* applicationUserName)
{
    if(purchaseBridge)
        purchaseBridge->setApplicationUsername(applicationUserName);
}

EXPORT_API void SPStore_SendTransactionUpdateEvents(bool shouldSend)
{
    if(purchaseBridge)
        purchaseBridge->sendTransactionUpdateEvents(shouldSend);
}

EXPORT_API void SPStore_EnableHighDetailLogs(bool shouldEnable)
{
    if(purchaseBridge)
        purchaseBridge->enableHighDetailLogs(shouldEnable);
}

EXPORT_API void SPStore_RequestProductData(const char* productIdentifiers)
{
    if(purchaseBridge)
        purchaseBridge->downloadProducts(productIdentifiers);
}

EXPORT_API void SPStore_PurchaseProduct(const char* productIdentifier)
{
    if(purchaseBridge)
        purchaseBridge->purchaseProduct(productIdentifier);
}

EXPORT_API void SPStore_ForceFinishPendingTransactions()
{
    if(purchaseBridge)
        purchaseBridge->forceFinishPendingTransactions();
}

EXPORT_API void SPStore_FinishPendingTransaction(const char* transactionIdentifier)
{
    if(purchaseBridge)
        purchaseBridge->finishPendingTransaction(transactionIdentifier);
}

