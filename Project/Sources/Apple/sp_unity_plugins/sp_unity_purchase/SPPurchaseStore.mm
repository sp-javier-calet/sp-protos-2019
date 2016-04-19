#include "SPPurchaseStore.h"
#include "UnityGameObject.h"
#include "SPPurchaseUtils.hpp"
#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <string>

//Enum type and values must match with enum in Unity
typedef NS_ENUM(NSUInteger, TransactionState) {
    // Transaction is being added to the server queue.
    TSPurchasing = 0,
    
    // Transaction is in queue, user has been charged.  Client should complete the transaction.
    TSPurchased = 1,
    
    // Transaction was cancelled or failed before being added to the server queue.
    TSFailed = 2,
    
    // Transaction was restored from user's purchase history.  Client should complete the transaction.
    TSRestored = 3,
    
    // The transaction is in the queue, but its final status is pending external action.
    TSDeferred = 4
};

@interface PlatformPurchaseServices : NSObject<SKProductsRequestDelegate, SKPaymentTransactionObserver>
{
    void* platformPurchaseCenter_;
}

@property(nonatomic, assign) void* platformInAppPurchase;
@property(strong, nonatomic) NSArray* products;
@property(strong, nonatomic) SKProductsRequest* request;
@property(strong, nonatomic) NSString* unityListenerName;

- (id)initWithUnityListener:(const char*)listenerName;

- (BOOL)isInitializingStoreProductsIds;
- (SKPaymentTransaction*)getPendingTransaction:(const char*)productId;
- (BOOL)isPendingTransaction:(const char*)productId;
- (void)consume:(const char*)productId;//UPDATE RETURN TYPE NEEDED (hydra::IAPResult)

- (void)startSKProductsRequest:(NSMutableSet*)productIdentifiers;
- (void)cancelRequest;

- (BOOL)purchaseProduct:(const char*)productIdentifier;
- (void)recordTransaction:(SKPaymentTransaction*)transaction;

- (void)storeDebugLog:(NSString*) logMsg;
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
    
    NSMutableString* jsonString = [NSMutableString stringWithString:@"["];
    int count = 0;
    for(SKProduct* product in self.products)
    {
        NSNumberFormatter* nf = [[NSNumberFormatter alloc] init];
        nf.locale = product.priceLocale;
        nf.numberStyle = NSNumberFormatterCurrencyStyle;
        NSString* localizedPrice = [nf stringFromNumber:product.price];
        NSString* currencySymbol = nf.currencySymbol;
        NSString* price = [NSString stringWithFormat:@"%@", product.price];
        
        NSDictionary* productData = @{
                                      @"productIdentifier":product.productIdentifier,
                                      @"localizedTitle":product.localizedTitle,
                                      @"localizedDescription":product.localizedDescription,
                                      @"price":price,
                                      @"currencySymbol":currencySymbol,
                                      @"formattedPrice":localizedPrice,
                                      };
        NSError *error;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:productData options:0 error:&error];
        
        if (jsonData)
        {
            if(count > 0)
            {
                [jsonString appendString:@","];
            }
            ++count;
            [jsonString appendString:[[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding]];
        }
        else
        {
            NSMutableString* errorMessage = [NSMutableString stringWithString:@"Error parsing product: "];
            [errorMessage appendString:error.localizedDescription];
            NSLog(@"%@", errorMessage);
        }
    }
    [jsonString appendString:@"]"];
    
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsReceived", [jsonString cStringUsingEncoding:NSUTF8StringEncoding]);
}

- (void)request:(SKRequest*)request didFailWithError:(NSError*)error
{
    _request = nil;
    
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsRequestDidFail", [error.localizedDescription cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark - Start Purchase

- (BOOL)purchaseProduct:(const char*)productId
{
    [self storeDebugLog:@"... TEST purchaseProduct"];
    
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
    
    NSString* errorDescription = [NSString stringWithFormat:@"Invalid product ID or product not loaded: %@", productIdentifier];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [errorDescription cStringUsingEncoding:NSUTF8StringEncoding]);
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

#pragma mark - SKPaymentTransactionObserver


// Called when there are transactions in the payment queue
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    [self storeDebugLog:[NSString stringWithFormat:@"... TEST updatedTransactions %lu", (unsigned long)transactions.count]];
    
    for(SKPaymentTransaction * transaction in transactions)
    {
        TransactionState ts = TSPurchasing;
        switch (transaction.transactionState )
        {
            case SKPaymentTransactionStatePurchasing:
                ts = TSPurchasing;
                break;
            case SKPaymentTransactionStateDeferred:
                ts = TSDeferred;
                break;
            case SKPaymentTransactionStatePurchased:
                ts = TSPurchased;
                break;
            case SKPaymentTransactionStateRestored:
                ts = TSRestored;
                break;
            case SKPaymentTransactionStateFailed:
                ts = TSFailed;
                break;
            default:
                break;
        }
        
        //Check if failed transaction to notify about error and continue with the next
        if(ts == TSFailed)
        {
            NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
            UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [errorDescription cStringUsingEncoding:NSUTF8StringEncoding]);
            continue;
        }
        
        NSString* transactionState = [NSString stringWithFormat:@"%lu", (unsigned long)ts];
        NSURL* receiptUrl = [[NSBundle mainBundle] appStoreReceiptURL];
        NSData* receiptData = [NSData dataWithContentsOfURL:receiptUrl];
        NSString* receiptBase64 = [receiptData base64EncodedStringWithOptions:0];
        
        NSDictionary* transactionData = @{
                                      @"productIdentifier":transaction.payment.productIdentifier,
                                      @"transactionIdentifier":transaction.transactionIdentifier ? transaction.transactionIdentifier : @"",
                                      @"base64EncodedReceipt":receiptBase64 ? receiptBase64 : @"",
                                      @"transactionState":transactionState
                                      };
        NSError *error;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:transactionData options:0 error:&error];
        
        if (jsonData)
        {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            
            [self storeDebugLog:jsonString];
            
            switch (ts)
            {
                case TSPurchased:
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchased", [jsonString cStringUsingEncoding:NSUTF8StringEncoding]);
                    break;
                default:
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("TransactionUpdated", [jsonString cStringUsingEncoding:NSUTF8StringEncoding]);
                    break;
            }
        }
        else
        {
            NSMutableString* errorMessage = [NSMutableString stringWithString:@"Error parsing transaction: "];
            [errorMessage appendString:error.localizedDescription];
            NSLog(@"%@", errorMessage);
        }
    }
}

// Logs all transactions that have been removed from the payment queue
- (void)paymentQueue:(SKPaymentQueue *)queue removedTransactions:(NSArray *)transactions
{
    [self storeDebugLog:@"... TEST removedTransactions"];
    
    for(SKPaymentTransaction * transaction in transactions)
    {
        NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
        NSLog(@"... TEST %@ was removed from the payment queue. Error: %@", transaction.payment.productIdentifier, errorDescription);
    }
}

#pragma mark - Debug

- (void)storeDebugLog:(NSString*) logMsg
{
    NSLog(@"Native Debug Log: %@", logMsg);
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("StoreDebugLog", [logMsg cStringUsingEncoding:NSUTF8StringEncoding]);
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
