//
//  SPPurchaseNative.m
//  sp_unity_plugins
//
//  Created by Andres Barrera on 20/04/16.
//
//

#include "SPPurchaseNativeServices.h"
#include "UnityGameObject.h"

@implementation SPPurchaseNativeServices

- (id)initWithUnityListener:(const char*)listenerName
{
    if((self = [super init]))
    {
        self.products = nil;
        self.request = nil;
        self.unityListenerName = [NSString stringWithUTF8String:listenerName];
        self.applicationUsername = nil;
        
        self.useApplicationUsername = false;
        self.useApplicationReceipt = false;
        self.canSendTransactionUpdateEvents = false;
        self.highDetailLogsEnabled = false;
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

- (void)setUseAppUsername:(BOOL) shouldUseAppUsername
{
    self.useApplicationUsername = shouldUseAppUsername;
    [self detailedLog:[NSString stringWithFormat:@"Using Application Username Setting: %@", self.useApplicationUsername ? @"true" : @"false"]];
}

- (void)setUseAppReceipt:(BOOL) shouldUseAppReceipt
{
    self.useApplicationReceipt = shouldUseAppReceipt;
    [self detailedLog:[NSString stringWithFormat:@"Using Application Receipt Setting: %@", self.useApplicationReceipt ? @"true" : @"false"]];
}

- (void)sendTransactionUpdateEvents:(BOOL) shouldSend
{
    self.canSendTransactionUpdateEvents = shouldSend;
}

- (void)enableHighDetailLogs:(BOOL) shouldEnable
{
    self.highDetailLogsEnabled = shouldEnable;
}

#pragma mark - Load Products

- (void)startProductsRequest:(NSMutableSet*)productIdentifiers
{
    [self detailedLog:[NSString stringWithFormat:@"Products Request Started"]];
    self.request = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    self.request.delegate = self;
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    [self.request start];
}

- (void)cancelRequest
{
    if(self.request != nil)
    {
        [self.request cancel];
    }
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
    [self detailedLog:[NSString stringWithFormat:@"Products Request Canceled"]];
}

- (BOOL)isInitializingStoreProductsIds
{
    return self.request != nil;
}

#pragma mark - Purchase Products

- (void)purchaseProduct:(const char*)productId
{
    NSString* productIdentifier = [NSString stringWithCString:productId encoding:NSUTF8StringEncoding];
    [self detailedLog:[NSString stringWithFormat:@"Purchasing Product %@", productIdentifier]];
    
    for(SKProduct* prod in self.products)
    {
        if([prod.productIdentifier isEqualToString:productIdentifier])
        {
            SKMutablePayment* payment = [SKMutablePayment paymentWithProduct:prod];
            if(self.useApplicationUsername && self.applicationUsername)
            {
                [self detailedLog:[NSString stringWithFormat:@"Using Application Username in Payment (%@)", self.applicationUsername]];
                payment.applicationUsername = self.applicationUsername;
            }
            [[SKPaymentQueue defaultQueue] addPayment:payment];
            return;
        }
    }
    
    //Error if no product with matching id was found
    NSString* errorDescription = [NSString stringWithFormat:@"Invalid product ID or product not loaded: %@", productIdentifier];
    [self detailedLog:[NSString stringWithFormat:@"Error Purchasing Product %@: %@", productIdentifier, errorDescription]];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [SPPurchaseNativeServices safeUTF8String:errorDescription]);
}

-(NSString*)getProductJson:(SKProduct*)product
{
    NSNumberFormatter* nf = [[NSNumberFormatter alloc] init];
    nf.locale = product.priceLocale;
    nf.numberStyle = NSNumberFormatterCurrencyStyle;
    NSString* localizedPrice = [nf stringFromNumber:product.price];
    NSString* currencySymbol = nf.currencySymbol;
    NSString* price = [NSString stringWithFormat:@"%@", product.price];
    
    NSDictionary* productData = @{
                                  @"productIdentifier":[SPPurchaseNativeServices safeNSString:product.productIdentifier],
                                  @"localizedTitle":[SPPurchaseNativeServices safeNSString:product.localizedTitle],
                                  @"localizedDescription":[SPPurchaseNativeServices safeNSString:product.localizedDescription],
                                  @"price":[SPPurchaseNativeServices safeNSString:price],
                                  @"currencySymbol":[SPPurchaseNativeServices safeNSString:currencySymbol],
                                  @"formattedPrice":[SPPurchaseNativeServices safeNSString:localizedPrice],
                                  };
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:productData options:0 error:&error];
    
    if (jsonData)
    {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    else
    {
        NSMutableString* errorMessage = [NSMutableString stringWithString:@"Error parsing product: "];
        [errorMessage appendString:error.localizedDescription];
        NSLog(@"%@", errorMessage);
        return nil;
    }
}

-(NSMutableString*)getProductsJson:(NSArray*)products
{
    NSMutableString* jsonString = [NSMutableString stringWithString:@"["];
    int count = 0;
    for(SKProduct* product in products)
    {
        NSString* productJson = [self getProductJson:product];
        if(productJson)
        {
            if(count > 0)
            {
                [jsonString appendString:@","];
            }
            ++count;
            [jsonString appendString:productJson];
        }
    }
    [jsonString appendString:@"]"];
    return jsonString;
}

#pragma mark - Transaction Operations

- (void)forceUpdatePendingTransactions
{
    [self detailedLog:[NSString stringWithFormat:@"Forcing Transactions Update"]];
    [self paymentQueue:[SKPaymentQueue defaultQueue] updatedTransactions:[[SKPaymentQueue defaultQueue] transactions]];
}

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
    SKPaymentTransaction* pendingTransaction = [self getPendingTransaction:transactionIdentifier];
    if(pendingTransaction)
    {
        [self detailedLog:[NSString stringWithFormat:@"Finishing Transaction %@", pendingTransaction.transactionIdentifier]];
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

- (void)forceFinishPendingTransactions
{
    [self detailedLog:[NSString stringWithFormat:@"Forcefull Finishing All Transactions"]];
    for(SKPaymentTransaction* pendingTransaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

-(NSString*)getTransactionJson:(SKPaymentTransaction*)transaction
{
    NSString* transactionState = [NSString stringWithFormat:@"%lu", (unsigned long)[self getTransactionStateEquivalence:transaction.transactionState]];
    NSData* receiptData = nil;
    if(self.useApplicationReceipt)
    {
        NSURL* receiptUrl = [[NSBundle mainBundle] appStoreReceiptURL];
        receiptData = [NSData dataWithContentsOfURL:receiptUrl];
    }
    else
    {
        receiptData = transaction.transactionReceipt;
    }
    NSString* receiptBase64 = [receiptData base64EncodedStringWithOptions:0];
    
    NSDictionary* transactionData = @{
                                      @"productIdentifier":[SPPurchaseNativeServices safeNSString:transaction.payment.productIdentifier],
                                      @"transactionIdentifier":[SPPurchaseNativeServices safeNSString:transaction.transactionIdentifier],
                                      @"base64EncodedReceipt":[SPPurchaseNativeServices safeNSString:receiptBase64],
                                      @"transactionState":[SPPurchaseNativeServices safeNSString:transactionState]
                                      };
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:transactionData options:0 error:&error];
    
    if (jsonData)
    {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    else
    {
        NSMutableString* errorMessage = [NSMutableString stringWithString:@"Error parsing transaction: "];
        [errorMessage appendString:error.localizedDescription];
        NSLog(@"%@", errorMessage);
        return nil;
    }
}

-(NSMutableString*)getTransactionsJson:(NSArray*)transactions
{
    return [self getTransactionsJson:transactions useFilter:false withFilter:SKPaymentTransactionStatePurchasing];
}

-(NSMutableString*)getTransactionsJson:(NSArray*)transactions withFilter:(SKPaymentTransactionState)filterState
{
    return [self getTransactionsJson:transactions useFilter:true withFilter:filterState];
}

-(NSMutableString*)getTransactionsJson:(NSArray*)transactions useFilter:(BOOL)doFilter withFilter:(SKPaymentTransactionState)filterState
{
    NSMutableString* jsonString = [NSMutableString stringWithString:@"["];
    int count = 0;
    for(SKPaymentTransaction * transaction in transactions)
    {
        if(doFilter && transaction.transactionState != filterState)
        {
            continue;
        }
        
        NSString* transactionJson = [self getTransactionJson:transaction];
        if(transactionJson)
        {
            if(count > 0)
            {
                [jsonString appendString:@","];
            }
            ++count;
            [jsonString appendString:transactionJson];
        }
    }
    [jsonString appendString:@"]"];
    return jsonString;
}

-(TransactionState)getTransactionStateEquivalence:(SKPaymentTransactionState)transactionState
{
    TransactionState ts = TSPurchasing;
    switch (transactionState)
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
    return ts;
}

#pragma mark - Debug

- (void)sendUnityDebugLog:(NSString*) logMsg
{
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("StoreDebugLog", [SPPurchaseNativeServices safeUTF8String:logMsg]);
}

- (void)detailedLog:(NSString*) logMsg
{
    if(self.highDetailLogsEnabled)
    {
        NSLog(@"[SP-IAP] %@", logMsg);
    }
}

+(NSString*)transactionStateToNSString:(SKPaymentTransactionState)transactionState
{
    switch (transactionState )
    {
        case SKPaymentTransactionStatePurchasing:
            return [NSString stringWithFormat:@"Purchasing"];
            break;
        case SKPaymentTransactionStateDeferred:
            return [NSString stringWithFormat:@"Deferred"];
            break;
        case SKPaymentTransactionStatePurchased:
            return [NSString stringWithFormat:@"Purchased"];
            break;
        case SKPaymentTransactionStateRestored:
            return [NSString stringWithFormat:@"Restored"];
            break;
        case SKPaymentTransactionStateFailed:
            return [NSString stringWithFormat:@"Failed"];
            break;
        default:
            return [NSString stringWithFormat:@"Invalid State"];
            break;
    }
}

#pragma mark - SKProductsRequestDelegate

- (void)productsRequest:(SKProductsRequest*)request didReceiveResponse:(SKProductsResponse*)response
{
    self.products = response.products;
    self.request = nil;
    [self detailedLog:[NSString stringWithFormat:@"Total Products Loaded %lu", (unsigned long)self.products.count]];
    
    NSMutableString* productsJson = [self getProductsJson:self.products];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsReceived", [SPPurchaseNativeServices safeUTF8String:productsJson]);
    [self detailedLog:[NSString stringWithFormat:@"Products Loaded: %@", productsJson]];
    
    //Force an update here for all transactions in queue. Because some of them may have been updated too early (before all listeners in Unity were set)
    [self forceUpdatePendingTransactions];
}

- (void)request:(SKRequest*)request didFailWithError:(NSError*)error
{
    _request = nil;
    [self detailedLog:[NSString stringWithFormat:@"Products Request Failed: %@", error.localizedDescription]];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsRequestDidFail", [SPPurchaseNativeServices safeUTF8String:error.localizedDescription]);
}

#pragma mark - SKPaymentTransactionObserver

// Called when there are transactions in the payment queue
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    [self detailedLog:[NSString stringWithFormat:@"Updated Transactions: %lu", (unsigned long)transactions.count]];
    for(SKPaymentTransaction * transaction in transactions)
    {
        [self detailedLog:[NSString stringWithFormat:@"Transaction %@ Updated (Product ID: %@). State: %@",
                           transaction.transactionIdentifier, transaction.payment.productIdentifier,
                           [SPPurchaseNativeServices transactionStateToNSString:transaction.transactionState]]];
        
        TransactionState ts = [self getTransactionStateEquivalence:transaction.transactionState];
        
        switch (ts)
        {
            case TSPurchased:
            {
                NSString* transactionJson = [self getTransactionJson:transaction];
                if (transactionJson)
                {
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchased", [SPPurchaseNativeServices safeUTF8String:transactionJson]);
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseAwaitingConfirmation", [SPPurchaseNativeServices safeUTF8String:transactionJson]);
                }
            }
                break;
                
            case TSFailed:
            {
                NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : [NSString stringWithFormat:@""];
                if (transaction.error.code == SKErrorPaymentCancelled)
                {
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseCancelled", "Payment Cancelled");
                }
                else
                {
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [SPPurchaseNativeServices safeUTF8String:errorDescription]);
                }
                [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
                continue;
            }
                break;
                
            default:
            {
                if(!self.canSendTransactionUpdateEvents)
                {
                    continue;
                }
                NSString* transactionJson = [self getTransactionJson:transaction];
                if (transactionJson)
                {
                    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("TransactionUpdated", [SPPurchaseNativeServices safeUTF8String:transactionJson]);
                }
            }
                break;
        }
    }
}

// Logs all transactions that have been removed from the payment queue
- (void)paymentQueue:(SKPaymentQueue *)queue removedTransactions:(NSArray *)transactions
{
    for(SKPaymentTransaction * transaction in transactions)
    {
        [self detailedLog:[NSString stringWithFormat:@"Transaction %@ (Product %@) was removed from the payment queue.", transaction.transactionIdentifier, transaction.payment.productIdentifier]];
    }
}

#pragma mark - Utils

+(NSString*)safeNSString:(NSString*)string
{
    if( string == nil )
    {
        return [NSString stringWithFormat:@""];
    }
    else
    {
        return string;
    }
}

+(const char *)safeUTF8String:(NSString*)string
{
    if( string == nil )
    {
        return "";
    }
    else
    {
        return [string UTF8String];
    }
}

+(const char *)safeCString:(NSString*)string usingEncoding:(NSStringEncoding)enc
{
    if( string == nil )
    {
        return "";
    }
    else
    {
        return [string cStringUsingEncoding:enc];
    }
}

@end