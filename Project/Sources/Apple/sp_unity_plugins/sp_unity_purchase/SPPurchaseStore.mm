#include "SPPurchaseStore.h"
#include "UnityGameObject.h"
#include "SPPurchaseUtils.hpp"
#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <string>

@interface PlatformPurchaseServices : NSObject<SKProductsRequestDelegate, SKPaymentTransactionObserver>
{
    void* platformPurchaseCenter_;
}

@property(nonatomic, assign) void* platformInAppPurchase;
@property(strong, nonatomic) NSArray* products;
@property(strong, nonatomic) SKProductsRequest* request;
@property(strong, nonatomic) NSString* unityListenerName;

- (id)initWithUnityListener:(const char*)listenerName;
- (id)initWithPlatformInAppPurchase:(void*)platformInAppPurchase;

- (BOOL)isInitializingStoreProductsIds;
- (SKPaymentTransaction*)getPendingTransaction:(const char*)productId;
- (BOOL)isPendingTransaction:(const char*)productId;
- (void)consume:(const char*)productId;//UPDATE RETURN TYPE NEEDED (hydra::IAPResult)

- (void)startSKProductsRequest:(NSMutableSet*)productIdentifiers;
- (void)cancelRequest;

- (BOOL)purchaseProduct:(const char*)productIdentifier;
- (void)recordTransaction:(SKPaymentTransaction*)transaction;

- (void)storeDebugLog:(const char*) logMsg;
@end

@implementation PlatformPurchaseServices

@synthesize platformInAppPurchase = platformPurchaseCenter_;

- (id)initWithUnityListener:(const char*)listenerName
{
    if((self = [super init]))
    {
        self.unityListenerName = [NSString stringWithUTF8String:listenerName];
    }
    
    return self;
}

- (id)initWithPlatformInAppPurchase:(void*)platformInAppPurchase
{
    if((self = [super init]))
    {
        self.platformInAppPurchase = platformInAppPurchase;
    }
    
    return self;
}

- (void)dealloc
{
    [self cancelRequest];
}

#pragma mark - getters

- (BOOL)isInitializingStoreProductsIds
{
    return self.request != nil;
}

#pragma mark - Initialize Purchase products

- (void)startSKProductsRequest:(NSMutableSet*)productIdentifiers
{
    self.request = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    self.request.delegate = self;
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    [self.request start];
}

- (void)cancelRequest
{
    [self.request cancel];
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
}

- (SKPaymentTransaction*)getPendingTransaction:(const char*)productId
{
    NSString* productIdentifierNS = [NSString stringWithUTF8String:productId];
    for(SKPaymentTransaction* transaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        if([transaction.payment.productIdentifier isEqualToString:productIdentifierNS])
        {
            return transaction;
        }
    }
    
    return nil;
}

- (BOOL)isPendingTransaction:(const char*)productId
{
    return [self getPendingTransaction:productId] != nil;
}

- (void)consume:(const char*)productId
{
    SKPaymentTransaction* pendingTransaction = [self getPendingTransaction:productId];
    if(pendingTransaction)
    {
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
        //return hydra::IAPResult(true);//UPDATE NEEDED
    }
    
    //return hydra::IAPResult("No pending transaction for " + productId);//UPDATE NEEDED
}


// Empty implementation in order to avoid unrecognized selector crashes...
// blind fix anti hacks
- (void)recordTransaction:(SKPaymentTransaction*)transaction
{
    assert("This method shouldn't be called." && false);
}

#pragma mark - SKProductsRequest Delegate

- (void)productsRequest:(SKProductsRequest*)request didReceiveResponse:(SKProductsResponse*)response
{
    self.products = response.products;
    self.request = nil;
    
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsReceived", "SUCCESS!");
    //[self updateDownloadedProducts];
}

- (void)updateDownloadedProducts
{
    //UPDATE NEEDED
    /*
    NSString* localizedPrice;
    NSString* name;
    
    hydra::VPurchaseProductDatas datas;
    
    for(SKProduct* product in self.products)
    {
        NSNumberFormatter* nf = [[NSNumberFormatter alloc] init];
        nf.locale = product.priceLocale;
        nf.numberStyle = NSNumberFormatterCurrencyStyle;
        localizedPrice = [nf stringFromNumber:product.price];
        name = product.localizedTitle;
        
        hydra::PurchaseProductData productData([NSStringExtra safeUTF8String:product.productIdentifier], [NSStringExtra safeUTF8String:name],
                                               [NSStringExtra safeUTF8String:localizedPrice], [product.price floatValue]);
        
        datas.push_back(productData);
    }
    getPurchaseCenter()->getStatusDelegate().onProductsUpdated(hydra::DownloadProductsState::Success, "", datas);
    //*/
}

- (void)request:(SKRequest*)request didFailWithError:(NSError*)error
{
    _request = nil;
    
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsRequestDidFail", [error.localizedDescription cStringUsingEncoding:NSUTF8StringEncoding]);
    //UPDATE NEEDED
    /*
    getPurchaseCenter()->getStatusDelegate().onProductsUpdated(
                                                               hydra::DownloadProductsState::Error, [NSStringExtra safeUTF8String:error.localizedDescription], {});
    //*/
}

#pragma mark - Start Purchase

- (BOOL)purchaseProduct:(const char*)productId
{
    NSString* productIdentifier = [NSString stringWithCString:productId encoding:NSUTF8StringEncoding];
    
    for(SKProduct* prod in self.products)
    {
        if([prod.productIdentifier isEqualToString:productIdentifier])
        {
            SKPayment* payment = [SKPayment paymentWithProduct:prod];
            [[SKPaymentQueue defaultQueue] addPayment:payment];
            
            return YES;
        }
    }
    
    return NO;
}

#pragma mark - Transactions State processing

- (void)consumeAsync:(SKPaymentTransaction*)transaction
{
    //UPDATE NEEDED
    /*
    std::string productId = [NSStringExtra safeUTF8String:transaction.payment.productIdentifier];
    std::string orderId = [NSStringExtra safeUTF8String:transaction.transactionIdentifier];
    std::string encodedReceipt = [NSStringExtra safeUTF8String:[PlatformPurchaseServices createEncodedString:[transaction transactionReceipt]]];
    
    NSDictionary *receiptDict = [self dictionaryFromPlistData:transaction.transactionReceipt];
    std::string signature = [NSStringExtra safeUTF8String:[receiptDict objectForKey:@"signature"]];
    
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0), ^{
        hydra::ValidationRequest request(productId, orderId, encodedReceipt, signature);
        hydra::ValidationResult result = getPurchaseCenter()->validate(request);
        
        // Create a purchase info with all data and result status
        hydra::PurchaseInfo purchaseInfo((result.code == 0)? hydra::PurchaseState::ValidateSuccess : hydra::PurchaseState::ValidateFailed);
        purchaseInfo.productId = productId;
        purchaseInfo.orderId = orderId;
        purchaseInfo.responseCode = result.code;
        purchaseInfo.message = result.message;
        purchaseInfo.payload = result.payload;
        
        getPurchaseCenter()->getStatusDelegate().onPurchaseUpdated(purchaseInfo);
    });
    //*/
}

- (NSDictionary *)dictionaryFromPlistData:(NSData *)data
{
    NSError *error;
    NSDictionary *dictionaryParsed = [NSPropertyListSerialization propertyListWithData:data
                                                                               options:NSPropertyListImmutable
                                                                                format:nil
                                                                                 error:&error];
    if (!dictionaryParsed)
    {
        if (error)
        {
            NSLog(@"Error parsing plist");
        }
        return nil;
    }
    return dictionaryParsed;
}

- (void)failedTransaction:(SKPaymentTransaction*)transaction
{
    //UPDATE NEEDED
    /*
    hydra::PurchaseInfo purchaseInfo(hydra::PurchaseState::PurchaseCanceled);
    purchaseInfo.productId = [NSStringExtra safeUTF8String:transaction.payment.productIdentifier];
    
    if(transaction.error.code != SKErrorPaymentCancelled)
    {
        NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
        purchaseInfo.message = errorDescription ? [NSStringExtra safeUTF8String:errorDescription] : "";
        purchaseInfo.state = hydra::PurchaseState::PurchaseFailed;
        
        DLog("IAP - Transaction Error: %s\n", purchaseInfo.message.c_str());
    }
    
    getPurchaseCenter()->getStatusDelegate().onPurchaseUpdated(purchaseInfo);
    
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
     //*/
}

#pragma mark - SKPaymentTransactionObserve

- (void)paymentQueue:(SKPaymentQueue*)queue updatedTransactions:(NSArray*)transactions
{
    //UPDATE NEEDED
    /*
    for(SKPaymentTransaction* transaction in transactions)
    {
        hydra::PurchaseInfo purchaseInfo(hydra::PurchaseState::PurchaseFinished);
        purchaseInfo.productId = [NSStringExtra safeUTF8String:transaction.payment.productIdentifier];
        
        NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
        purchaseInfo.message = errorDescription ? [NSStringExtra safeUTF8String:errorDescription] : "";
        
        switch(transaction.transactionState)
        {
            case SKPaymentTransactionStatePurchased:
                DLog("IAP - Complete Transaction...\n");
                [self consumeAsync:transaction];
                break;
                
            case SKPaymentTransactionStateFailed:
                DLog("IAP - Failed Transaction...\n");
                [self failedTransaction:transaction];
                break;
                
            default:
                break;
        }
    }
     //*/
}

- (void)paymentQueue:(SKPaymentQueue*)queue removedTransactions:(NSArray*)transactions
{
    //UPDATE NEEDED
    /*
    DLog("IAP - Removed Transaction...\n");
    for(SKPaymentTransaction* transaction in transactions)
    {
        hydra::PurchaseInfo purchaseInfo(hydra::PurchaseState::RemovedTransaction);
        purchaseInfo.productId = [NSStringExtra safeUTF8String:transaction.payment.productIdentifier];
        
        NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
        purchaseInfo.message = errorDescription ? [NSStringExtra safeUTF8String:errorDescription] : "";
        
        getPurchaseCenter()->getStatusDelegate().onPurchaseUpdated(purchaseInfo);
    }
     //*/
}

#pragma mark - Debug

- (void)storeDebugLog:(const char*) logMsg
{
    NSLog(@"Native Debug Log: %@", [NSString stringWithUTF8String:logMsg]);
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("StoreDebugLog", logMsg);
}

#pragma mark - Receipt Encoding

// function needed to encode Purchase Receipt and send to Server to validate
+ (NSString*)createEncodedString:(NSData*)data
{
    static char table[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    
    const ssize_t size = ((data.length + 2) / 3) * 4;
    uint8_t output[size];
    
    const uint8_t* input = (const uint8_t*)[data bytes];
    for(unsigned i = 0; i < data.length; i += 3)
    {
        int value = 0;
        for(unsigned j = i; j < (i + 3); j++)
        {
            value <<= 8;
            if(j < data.length)
                value |= (0xFF & input[j]);
        }
        
        const int index = (i / 3) * 4;
        output[index + 0] = table[(value >> 18) & 0x3F];
        output[index + 1] = table[(value >> 12) & 0x3F];
        output[index + 2] = (i + 1) < data.length ? table[(value >> 6) & 0x3F] : '=';
        output[index + 3] = (i + 2) < data.length ? table[(value >> 0) & 0x3F] : '=';
    }
    
    return [[NSString alloc] initWithBytes:output length:size encoding:NSASCIIStringEncoding];
}

@end

/* BRIDGE */

class PlatformPurchaseCenterBridge
{
    PlatformPurchaseServices* _purchaseServices;
    
public:
    PlatformPurchaseCenterBridge(const char* listenerObjectName)
    : _purchaseServices(nullptr)
    {
        _purchaseServices = [[PlatformPurchaseServices alloc] initWithUnityListener:listenerObjectName];
    }
    
    void downloadProducts(const std::string& productList)
    {
        //DEBUG
        [_purchaseServices storeDebugLog:"*** TEST Downloading Products"];
        [_purchaseServices storeDebugLog:productList.c_str()];
        
        if([_purchaseServices isInitializingStoreProductsIds])
        {
            return;//Already initializing products
        }
        
        NSMutableSet* productIdentifiers = [[NSMutableSet alloc] init];
        
        std::vector<std::string> products = SPPurchaseUtils::split(productList, ',');
        for(std::string& productIdentifier : products)
        {
            NSString* productId = [NSString stringWithCString:productIdentifier.c_str() encoding:NSUTF8StringEncoding];
            [productIdentifiers addObject:productId];
        }
        
        [_purchaseServices startSKProductsRequest:productIdentifiers];
    }
    
    void purchaseProduct(const char* productIdentifier)
    {
        //DEBUG
        [_purchaseServices storeDebugLog:"*** TEST Purchasing Product"];
        [_purchaseServices storeDebugLog:productIdentifier];
        
        [_purchaseServices purchaseProduct:productIdentifier];
    }
    
    
    /*IAPResult isPendingTransaction(const std::string& productIdentifier) const
    {
        return IAPResult([_purchaseServices isPendingTransaction:productIdentifier]);
    }
    
    IAPResult consume(const std::string& productId) const
    {
        return [_purchaseServices consume:productId];
    }*/
};

PlatformPurchaseCenterBridge* purchaseBridge;

/* EXPORT FUNCTIONS */

EXPORT_API void SPStore_Init(const char* listenerObjectName)
{
    purchaseBridge = new PlatformPurchaseCenterBridge(listenerObjectName);
}

EXPORT_API bool SPStore_CanMakePayments()
{
    return false;
}

EXPORT_API void SPStore_SetApplicationUsername(const char* applicationUserName)
{
    
}

EXPORT_API const char* SPStore_GetAppStoreReceiptUrl()
{
    return "";
}

EXPORT_API void SPStore_SendTransactionUpdateEvents(bool sendTransactionUpdateEvents)
{
    
}

EXPORT_API void SPStore_EnableHighDetailLogs(bool shouldEnable)
{
    
}

EXPORT_API void SPStore_RequestProductData(const char* productIdentifiers)
{
    purchaseBridge->downloadProducts(productIdentifiers);
}

EXPORT_API void SPStore_PurchaseProduct(const char* productIdentifier)
{
    purchaseBridge->purchaseProduct(productIdentifier);
}

EXPORT_API void SPStore_FinishPendingTransactions()
{
    
}

EXPORT_API void SPStore_ForceFinishPendingTransactions()
{
    
}

EXPORT_API void SPStore_FinishPendingTransaction(const char* transactionIdentifier)
{
    
}

EXPORT_API void SPStore_PauseDownloads()
{
    
}

EXPORT_API void SPStore_ResumeDownloads()
{
    
}

EXPORT_API void SPStore_CancelDownloads()
{
    
}

EXPORT_API void SPStore_RestoreCompletedTransactions()
{
    
}

EXPORT_API const char* SPStore_GetAllSavedTransactions()
{
    return "";
}

EXPORT_API void SPStore_DisplayStoreWithProductId(const char* productId, const char* affiliateToken)
{
    
}
