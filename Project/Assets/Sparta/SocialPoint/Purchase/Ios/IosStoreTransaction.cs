using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public enum IosStoreTransactionState
    {
        // Transaction is being added to the server queue.
        Purchasing,

        // Transaction is in queue, user has been charged.  Client should complete the transaction.
        Purchased,

        // Transaction was cancelled or failed before being added to the server queue.
        Failed,

        // Transaction was restored from user's purchase history.  Client should complete the transaction.
        Restored,

        // The transaction is in the queue, but its final status is pending external action.
        Deferred
    }

    public class IosStoreTransaction
    {
        public string ProductIdentifier { get; private set; }

        public string TransactionIdentifier { get; private set; }

        public string Base64EncodedTransactionReceipt { get; private set; }

        public IosStoreTransactionState TransactionState { get; private set; }



        public static List<IosStoreTransaction> TransactionsFromJson(string json)
        {
            return IosStoreAttrUtils.IosStoreListFromJson<IosStoreTransaction>(json, TransactionFromDictionary);
        }


        public static IosStoreTransaction TransactionFromJson(string json)
        {
            return IosStoreAttrUtils.IosStoreObjectFromJson<IosStoreTransaction>(json, TransactionFromDictionary);
        }


        public static IosStoreTransaction TransactionFromDictionary(AttrDic dict)
        {
            var transaction = new IosStoreTransaction();

            if(dict.ContainsKey("productIdentifier"))
                transaction.ProductIdentifier = dict["productIdentifier"].ToString();

            if(dict.ContainsKey("transactionIdentifier"))
                transaction.TransactionIdentifier = dict["transactionIdentifier"].ToString();

            if(dict.ContainsKey("base64EncodedReceipt"))
                transaction.Base64EncodedTransactionReceipt = dict["base64EncodedReceipt"].ToString();

            if(dict.ContainsKey("transactionState"))
                transaction.TransactionState = (IosStoreTransactionState)int.Parse(dict["transactionState"].ToString());

            return transaction;
        }


        public override string ToString()
        {
            return string.Format("<IosStoreTransaction> ID: {0}, transactionIdentifier: {1}, transactionState: {2}",
                ProductIdentifier, TransactionIdentifier, TransactionState);
        }

    }
}
#endif
