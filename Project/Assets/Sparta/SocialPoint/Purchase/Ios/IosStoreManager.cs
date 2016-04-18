using UnityEngine;
using System;
using System.Collections.Generic;


#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public class IosStoreManager : MonoBehaviour
    {
        public static bool autoConfirmTransactions = true;

        // Fired when the product list your required returns. Automatically serializes the productString into IosStoreProduct's.
        public static event Action<List<IosStoreProduct>> ProductListReceivedEvent;

        // Fired when requesting product data fails
        public static event Action<string> ProductListRequestFailedEvent;

        // Fired anytime Apple updates a transaction if you called setShouldSendTransactionUpdateEvents with true. Check the transaction.transactionState to
        // know what state the transaction is currently in.
        public static event Action<IosStoreTransaction> TransactionUpdatedEvent;

        // Fired when a product purchase has returned from Apple's servers and is awaiting completion. By default the plugin will finish transactions for you.
        // You can change that behavior by setting autoConfirmTransactions to false which then requires that you call IosStoreBinding.finishPendingTransaction
        // to complete a purchase.
        public static event Action<IosStoreTransaction> ProductPurchaseAwaitingConfirmationEvent;

        // Fired when a product is successfully paid for. The event will provide a IosStoreTransaction object that holds the productIdentifer and receipt of the purchased product.
        public static event Action<IosStoreTransaction> PurchaseSuccessfulEvent;

        // Fired when a product purchase fails
        public static event Action<string> PurchaseFailedEvent;

        // Fired when a product purchase is cancelled by the user or system
        public static event Action<string> PurchaseCancelledEvent;

        // Fired when all transactions from the user's purchase history have successfully been added back to the queue. Note that this event will almost always
        // fire before each individual transaction is processed.
        public static event Action RestoreTransactionsFinishedEvent;

        // Fired when an error is encountered while adding transactions from the user's purchase history back to the queue
        public static event Action<string> RestoreTransactionsFailedEvent;

        // Fired when any SKDownload objects are updated by Apple. If using hosted content you should not be confirming the transaction until all downloads are complete.
        public static event Action<List<IosStoreDownload>> PaymentQueueUpdatedDownloadsEvent;



        static IosStoreManager()
        {
            //UPDATE NEEDED!
            //AbstractManager.initialize(typeof(IosStoreManager));

            // we ignore the results of this call because our only purpose is to trigger the creation of the required listener on the native side for transaction processing.
            //IosStoreBinding.canMakePayments();
        }


        public void transactionUpdated(string json)
        {
            if(TransactionUpdatedEvent != null)
                TransactionUpdatedEvent(IosStoreTransaction.TransactionFromJson(json));
        }


        public void productPurchaseAwaitingConfirmation(string json)
        {
            if(ProductPurchaseAwaitingConfirmationEvent != null)
                ProductPurchaseAwaitingConfirmationEvent(IosStoreTransaction.TransactionFromJson(json));

            if(autoConfirmTransactions)
                IosStoreBinding.FinishPendingTransactions();
        }


        public void productPurchased(string json)
        {
            if(PurchaseSuccessfulEvent != null)
                PurchaseSuccessfulEvent(IosStoreTransaction.TransactionFromJson(json));
        }


        public void productPurchaseFailed(string error)
        {
            if(PurchaseFailedEvent != null)
                PurchaseFailedEvent(error);
        }


        public void productPurchaseCancelled(string error)
        {
            if(PurchaseCancelledEvent != null)
                PurchaseCancelledEvent(error);
        }


        public void productsReceived(string json)
        {
            if(ProductListReceivedEvent != null)
                ProductListReceivedEvent(IosStoreProduct.ProductsFromJson(json));
        }


        public void productsRequestDidFail(string error)
        {
            if(ProductListRequestFailedEvent != null)
                ProductListRequestFailedEvent(error);
        }


        public void restoreCompletedTransactionsFailed(string error)
        {
            if(RestoreTransactionsFailedEvent != null)
                RestoreTransactionsFailedEvent(error);
        }


        public void restoreCompletedTransactionsFinished(string empty)
        {
            if(RestoreTransactionsFinishedEvent != null)
                RestoreTransactionsFinishedEvent();
        }


        public void paymentQueueUpdatedDownloads(string json)
        {
            if(PaymentQueueUpdatedDownloadsEvent != null)
                PaymentQueueUpdatedDownloadsEvent(IosStoreDownload.DownloadsFromJson(json));

        }

    }
}
#endif
