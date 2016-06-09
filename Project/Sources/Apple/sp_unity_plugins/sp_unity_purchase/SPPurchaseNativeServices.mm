//
//  SPPurchaseNative.m
//  sp_unity_plugins
//
//  Created by Andres Barrera on 20/04/16.
//
//

#include "SPPurchaseNativeServices.h"
#include "UnityGameObject.h"
#include "SPUnityNativeUtils.h"
#include "SPNativeCallsSender.h"

@implementation SPPurchaseNativeServices

- (id)init
{
    if((self = [super init]))
    {
        self.products = nil;
        self.request = nil;
        self.applicationUsername = nil;
        
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

#pragma mark - Product Operations

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

- (void)purchaseProduct:(const char*)productId
{
    NSString* productIdentifier = [NSString stringWithCString:productId encoding:NSUTF8StringEncoding];
    [self detailedLog:[NSString stringWithFormat:@"Purchasing Product %@", productIdentifier]];
    
    for(SKProduct* prod in self.products)
    {
        if([prod.productIdentifier isEqualToString:productIdentifier])
        {
            SKMutablePayment* payment = [SKMutablePayment paymentWithProduct:prod];
            if(self.applicationUsername && SPUnityNativeUtils::isSystemVersionGreaterThanOrEqualTo(SPUnityNativeUtils::kV7))
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
    SPNativeCallsSender::SendMessage("ProductPurchaseFailed", [SPPurchaseNativeServices safeUTF8String:errorDescription]);
}

-(NSDictionary*)getProductJson:(SKProduct*)product
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
    return productData;
}

-(NSString*)getProductJsonString:(SKProduct*)product
{
    return [SPPurchaseNativeServices jsonDicitonaryToString:[self getProductJson:product]];
}

-(NSString*)getProductsListJsonString:(NSArray*)products
{
    
    NSMutableArray *array = [[NSMutableArray alloc]init];
    
    for(SKProduct* product in products)
    {
        NSDictionary* productJson = [self getProductJson:product];
        if(productJson)
        {
            [array addObject:productJson];
        }
    }
    
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:array options:0 error:&error];
    
    if (jsonData)
    {
        return [SPPurchaseNativeServices jsonDataToString:jsonData];
    }
    else
    {
        NSString* errorMessage = [NSString stringWithFormat:@"Error parsing product list: %@", error.localizedDescription];
        NSLog(@"%@", errorMessage);
        return @"[]";
    }
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

-(NSDictionary*)getTransactionJson:(SKPaymentTransaction*)transaction
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
    return transactionData;
}

-(NSString*)getTransactionJsonString:(SKPaymentTransaction*)transaction
{
    return [SPPurchaseNativeServices jsonDicitonaryToString:[self getTransactionJson:transaction]];
}

-(NSString*)getTransactionsListJsonString:(NSArray*)transactions
{
    return [self getTransactionsListJsonString:transactions useFilter:false withFilter:SKPaymentTransactionStatePurchasing];
}

-(NSString*)getTransactionsListJsonString:(NSArray*)transactions withFilter:(SKPaymentTransactionState)filterState
{
    return [self getTransactionsListJsonString:transactions useFilter:true withFilter:filterState];
}

-(NSString*)getTransactionsListJsonString:(NSArray*)transactions useFilter:(BOOL)doFilter withFilter:(SKPaymentTransactionState)filterState
{
    NSMutableArray *array = [[NSMutableArray alloc]init];
    
    for(SKPaymentTransaction * transaction in transactions)
    {
        NSDictionary* transactionJson = [self getTransactionJson:transaction];
        if(transactionJson)
        {
            [array addObject:transactionJson];
        }
    }
    
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:array options:0 error:&error];
    
    if (jsonData)
    {
        return [SPPurchaseNativeServices jsonDataToString:jsonData];
    }
    else
    {
        NSString* errorMessage = [NSString stringWithFormat:@"Error parsing product list: %@", error.localizedDescription];
        NSLog(@"%@", errorMessage);
        return @"[]";
    }
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
    SPNativeCallsSender::SendMessage("StoreDebugLog", [SPPurchaseNativeServices safeUTF8String:logMsg]);
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
    
    NSString* productsJson = [self getProductsListJsonString:self.products];
    SPNativeCallsSender::SendMessage("ProductsReceived", [SPPurchaseNativeServices safeUTF8String:productsJson]);
    [self detailedLog:[NSString stringWithFormat:@"Products Loaded: %@", productsJson]];
    
    //Force an update here for all transactions in queue. Because some of them may have been updated too early (before all listeners in Unity were set)
    [self forceUpdatePendingTransactions];
}

- (void)request:(SKRequest*)request didFailWithError:(NSError*)error
{
    _request = nil;
    [self detailedLog:[NSString stringWithFormat:@"Products Request Failed: %@", error.localizedDescription]];
    SPNativeCallsSender::SendMessage("ProductsRequestDidFail", [SPPurchaseNativeServices safeUTF8String:error.localizedDescription]);
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
                NSString* transactionJson = [self getTransactionJsonString:transaction];
                if (transactionJson)
                {
                    SPNativeCallsSender::SendMessage("ProductPurchased", [SPPurchaseNativeServices safeUTF8String:transactionJson]);
                }
            }
                break;
                
            case TSFailed:
            {
                NSString* errorDescription = transaction.error ? transaction.error.localizedDescription : [NSString stringWithFormat:@""];
                if (transaction.error.code == SKErrorPaymentCancelled)
                {
                    SPNativeCallsSender::SendMessage("ProductPurchaseCancelled", "Payment Cancelled");
                }
                else
                {
                    SPNativeCallsSender::SendMessage("ProductPurchaseFailed", [SPPurchaseNativeServices safeUTF8String:errorDescription]);
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
                NSString* transactionJson = [self getTransactionJsonString:transaction];
                if (transactionJson)
                {
                    SPNativeCallsSender::SendMessage("TransactionUpdated", [SPPurchaseNativeServices safeUTF8String:transactionJson]);
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

+(NSString*)jsonDicitonaryToString:(NSDictionary*)jsonObject
{
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:jsonObject options:0 error:&error];
    
    if (jsonData)
    {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    else
    {
        NSString* errorMessage = [NSString stringWithFormat:@"Error parsing json object: %@", error.localizedDescription];
        NSLog(@"%@", errorMessage);
        return nil;
    }
}

+(NSString*)jsonDataToString:(NSData*)jsonData
{
    if (jsonData)
    {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
    else
    {
        return nil;
    }
}

+(NSString*)safeNSString:(NSString*)string
{
    if( string == nil )
    {
        return @"";
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