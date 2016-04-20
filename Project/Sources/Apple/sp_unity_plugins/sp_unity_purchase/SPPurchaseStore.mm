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
@property(strong, nonatomic) NSString* applicationUsername;

- (id)initWithUnityListener:(const char*)listenerName;

- (void)setAppUsername:(const char*) userIdentifier;

- (void)startProductsRequest:(NSMutableSet*)productIdentifiers;
- (void)cancelRequest;
- (BOOL)isInitializingStoreProductsIds;

- (void)purchaseProduct:(const char*)productIdentifier;

- (SKPaymentTransaction*)getPendingTransaction:(const char*)transactionIdentifier;
- (void)finishPendingTransaction:(const char*)transactionIdentifier;
- (void)finishPendingTransactions;
- (void)forceFinishPendingTransactions;

- (void)storeDebugLog:(NSString*) logMsg;
@end

@implementation PlatformPurchaseServices

@synthesize platformInAppPurchase = platformPurchaseCenter_;

- (id)initWithUnityListener:(const char*)listenerName
{
    if((self = [super init]))
    {
        self.applicationUsername = nil;
        self.unityListenerName = [NSString stringWithUTF8String:listenerName];
    }
    
    return self;
}

- (void)dealloc
{
    [self cancelRequest];
}

#pragma mark - setters

- (void)setAppUsername:(const char*) userIdentifier
{
    self.applicationUsername = [NSString stringWithUTF8String:userIdentifier];
}

#pragma mark - Load Products

- (void)startProductsRequest:(NSMutableSet*)productIdentifiers
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

- (BOOL)isInitializingStoreProductsIds
{
    return self.request != nil;
}

#pragma mark - Purchase Products

- (void)purchaseProduct:(const char*)productId
{
    [self storeDebugLog:@"... TEST purchaseProduct"];
    
    NSString* productIdentifier = [NSString stringWithCString:productId encoding:NSUTF8StringEncoding];
    
    for(SKProduct* prod in self.products)
    {
        if([prod.productIdentifier isEqualToString:productIdentifier])
        {
            SKMutablePayment* payment = [SKMutablePayment paymentWithProduct:prod];
            if(self.applicationUsername)
            {
                payment.applicationUsername = self.applicationUsername;
            }
            [[SKPaymentQueue defaultQueue] addPayment:payment];
            return;
        }
    }
    
    //Error if no product with matching id was found
    NSString* errorDescription = [NSString stringWithFormat:@"Invalid product ID or product not loaded: %@", productIdentifier];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [errorDescription cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark - Transaction Operations

- (SKPaymentTransaction*)getPendingTransaction:(const char*)transactionIdentifier
{
    NSString* transactionIdentifierNS = [NSString stringWithUTF8String:transactionIdentifier];
    for(SKPaymentTransaction* transaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        if([transaction.transactionIdentifier isEqualToString:transactionIdentifierNS])
        {
            return transaction;
        }
    }
    
    return nil;
}

- (void)finishPendingTransaction:(const char*)transactionIdentifier
{
    [self storeDebugLog:@"... TEST finishPendingTransaction"];
    
    SKPaymentTransaction* pendingTransaction = [self getPendingTransaction:transactionIdentifier];
    if(pendingTransaction)
    {
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

- (void)finishPendingTransactions
{
    [self storeDebugLog:@"... TEST finishPendingTransactions"];
    
    for(SKPaymentTransaction* pendingTransaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        if (pendingTransaction.transactionState == SKPaymentTransactionStateFailed
            || pendingTransaction.transactionState == SKPaymentTransactionStateRestored
            || pendingTransaction.transactionState == SKPaymentTransactionStatePurchased)
        {
            [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
        }
    }
}

- (void)forceFinishPendingTransactions
{
    [self storeDebugLog:@"... TEST forceFinishPendingTransactions"];
    
    for(SKPaymentTransaction* pendingTransaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

#pragma mark - SKProductsRequestDelegate

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
            [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
            UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [errorDescription cStringUsingEncoding:NSUTF8StringEncoding]);
            continue;
        }
        
        NSString* transactionState = [NSString stringWithFormat:@"%lu", (unsigned long)ts];
        NSURL* receiptUrl = [[NSBundle mainBundle] appStoreReceiptURL];
        NSData* receiptData = [NSData dataWithContentsOfURL:receiptUrl];
        NSString* receiptBase64 = [PlatformPurchaseServices createEncodedString:receiptData];// [receiptData base64EncodedStringWithOptions:0];
        
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
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseAwaitingConfirmation", [jsonString cStringUsingEncoding:NSUTF8StringEncoding]);
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
        //TODO: SendMessage ProductPurchaseCancelled??
        NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : nil;
        NSLog(@"... TEST %@ was removed from the payment queue. Error: %@", transaction.payment.productIdentifier, errorDescription);
    }
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

#pragma mark - Debug

- (void)storeDebugLog:(NSString*) logMsg
{
    NSLog(@"Native Debug Log: %@", logMsg);
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("StoreDebugLog", [logMsg cStringUsingEncoding:NSUTF8StringEncoding]);
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
    
    void setApplicationUsername(const char* userIdentifier)
    {
        [_purchaseServices setAppUsername:userIdentifier];
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
        
        [_purchaseServices startProductsRequest:productIdentifiers];
    }
    
    void purchaseProduct(const char* productIdentifier)
    {
        [_purchaseServices purchaseProduct:productIdentifier];
    }
    
    void finishPendingTransaction(const char* productIdentifier)
    {
        return [_purchaseServices finishPendingTransaction:productIdentifier];
    }
    
    void finishPendingTransactions()
    {
        return [_purchaseServices finishPendingTransactions];
    }
    
    void forceFinishPendingTransactions()
    {
        return [_purchaseServices forceFinishPendingTransactions];
    }
};

PlatformPurchaseCenterBridge* purchaseBridge;

/* EXPORT FUNCTIONS */

EXPORT_API void SPStore_Init(const char* listenerObjectName)
{
    purchaseBridge = new PlatformPurchaseCenterBridge(listenerObjectName);
}

EXPORT_API void SPStore_SetApplicationUsername(const char* applicationUserName)
{
    purchaseBridge->setApplicationUsername(applicationUserName);
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
    purchaseBridge->finishPendingTransactions();
}

EXPORT_API void SPStore_ForceFinishPendingTransactions()
{
    purchaseBridge->forceFinishPendingTransactions();
}

EXPORT_API void SPStore_FinishPendingTransaction(const char* transactionIdentifier)
{
    purchaseBridge->finishPendingTransaction(transactionIdentifier);
}

