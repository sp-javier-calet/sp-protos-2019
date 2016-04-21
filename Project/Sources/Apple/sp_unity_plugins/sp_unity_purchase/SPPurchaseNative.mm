//
//  SPPurchaseNative.m
//  sp_unity_plugins
//
//  Created by Andres Barrera on 20/04/16.
//
//

#include "SPPurchaseNative.h"
#include "UnityGameObject.h"

@implementation PlatformPurchaseServices

- (id)initWithUnityListener:(const char*)listenerName
{
    if((self = [super init]))
    {
        self.applicationUsername = nil;
        self.useAppReceipt = false;
        self.canSendTransactionUpdateEvents = false;
        self.highDetailLogsEnabled = false;
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

- (void)setUseAppReceipt:(BOOL) shouldUseAppReceipt
{
    self.useApplicationReceipt = shouldUseAppReceipt;
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
    self.request = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    self.request.delegate = self;
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
    [self detailedLog:[NSString stringWithFormat:@"Products Request Started"]];
    [self.request start];
}

- (void)cancelRequest
{
    [self.request cancel];
    [self detailedLog:[NSString stringWithFormat:@"Products Request Canceled"]];
    [[SKPaymentQueue defaultQueue] removeTransactionObserver:self];
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
    [self detailedLog:[NSString stringWithFormat:@"Error Purchasing Product %@: %@", productIdentifier, errorDescription]];
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
    SKPaymentTransaction* pendingTransaction = [self getPendingTransaction:transactionIdentifier];
    if(pendingTransaction)
    {
        [self detailedLog:[NSString stringWithFormat:@"Finishing Transaction %@", pendingTransaction.transactionIdentifier]];
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

- (void)forceFinishPendingTransactions
{
    [self detailedLog:[NSString stringWithFormat:@"Forcefull Finishing All Transaction"]];
    for(SKPaymentTransaction* pendingTransaction in [[SKPaymentQueue defaultQueue] transactions])
    {
        [[SKPaymentQueue defaultQueue] finishTransaction:pendingTransaction];
    }
}

#pragma mark - Debug

- (void)sendUnityDebugLog:(NSString*) logMsg
{
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("StoreDebugLog", [logMsg cStringUsingEncoding:NSUTF8StringEncoding]);
}

- (void)detailedLog:(NSString*) logMsg
{
    if(self.highDetailLogsEnabled)
    {
        NSLog(@"[SP-IAP] %@", logMsg);
    }
}

#pragma mark - SKProductsRequestDelegate

- (void)productsRequest:(SKProductsRequest*)request didReceiveResponse:(SKProductsResponse*)response
{
    self.products = response.products;
    self.request = nil;
    [self detailedLog:[NSString stringWithFormat:@"Total Products Loaded %lu", (unsigned long)self.products.count]];
    
    NSMutableString* jsonString = [NSMutableString stringWithString:@"["];
    int count = 0;
    for(SKProduct* product in self.products)
    {
        [self detailedLog:[NSString stringWithFormat:@"Product Loaded: %@", product.productIdentifier]];
        
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
    [self detailedLog:[NSString stringWithFormat:@"Products Request Failed: %@", error.localizedDescription]];
    UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductsRequestDidFail", [error.localizedDescription cStringUsingEncoding:NSUTF8StringEncoding]);
}

#pragma mark - SKPaymentTransactionObserver

// Called when there are transactions in the payment queue
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions
{
    for(SKPaymentTransaction * transaction in transactions)
    {
        [self detailedLog:[NSString stringWithFormat:@"Transaction %@ Updated (Product ID: %@). State: %lu",
                           transaction.transactionIdentifier, transaction.payment.productIdentifier, (unsigned long)transaction.transactionState]];
        
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
            if (transaction.error.code == SKErrorPaymentCancelled)
            {
                UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseCancelled", "Payment Cancelled");
            }
            else
            {
                UnityGameObject(self.unityListenerName.UTF8String).SendMessage("ProductPurchaseFailed", [errorDescription cStringUsingEncoding:NSUTF8StringEncoding]);
            }
            [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
            continue;
        }
        else if(ts != TSPurchased && !self.canSendTransactionUpdateEvents)
        {
            continue;
        }
        
        NSString* transactionState = [NSString stringWithFormat:@"%lu", (unsigned long)ts];
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
    for(SKPaymentTransaction * transaction in transactions)
    {
        [self detailedLog:[NSString stringWithFormat:@"Transaction %@ (Product %@) was removed from the payment queue.", transaction.transactionIdentifier, transaction.payment.productIdentifier]];
    }
}

@end