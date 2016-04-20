//
//  SPPurchaseNative.h
//  sp_unity_plugins
//
//  Created by Andres Barrera on 20/04/16.
//
//

#ifndef SPPurchaseNative_h
#define SPPurchaseNative_h

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

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

@property(strong, nonatomic) NSArray* products;
@property(strong, nonatomic) SKProductsRequest* request;
@property(strong, nonatomic) NSString* unityListenerName;
@property(strong, nonatomic) NSString* applicationUsername;
@property(nonatomic) BOOL canSendTransactionUpdateEvents;
@property(nonatomic) BOOL highDetailLogsEnabled;

- (id)initWithUnityListener:(const char*)listenerName;

- (void)setAppUsername:(const char*) userIdentifier;
- (void)sendTransactionUpdateEvents:(BOOL) shouldSend;
- (void)enableHighDetailLogs:(BOOL) shouldEnable;

- (void)startProductsRequest:(NSMutableSet*)productIdentifiers;
- (void)cancelRequest;
- (BOOL)isInitializingStoreProductsIds;

- (void)purchaseProduct:(const char*)productIdentifier;

- (SKPaymentTransaction*)getPendingTransaction:(const char*)transactionIdentifier;
- (void)finishPendingTransaction:(const char*)transactionIdentifier;
- (void)forceFinishPendingTransactions;

- (void)sendUnityDebugLog:(NSString*) logMsg;
- (void)detailedLog:(NSString*) logMsg;

@end

#endif /* SPPurchaseNative_h */
