using System.Collections.Generic;
using SocialPoint.Attributes;

#if (UNITY_IOS || UNITY_TVOS)
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

    public sealed class IosStoreTransaction
    {
        public string ProductIdentifier { get; private set; }

        public string TransactionIdentifier { get; private set; }

        public string Base64EncodedTransactionReceipt { get; private set; }

        public IosStoreTransactionState TransactionState { get; private set; }

        const string ProductIdentifierKey = "productIdentifier";
        const string TransactionIdentifierKey = "transactionIdentifier";
        const string Base64EncodedReceiptKey = "base64EncodedReceipt";
        const string TransactionStateKey = "transactionState";


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

            if(dict.ContainsKey(ProductIdentifierKey))
            {
                transaction.ProductIdentifier = dict[ProductIdentifierKey].ToString();
            }

            if(dict.ContainsKey(TransactionIdentifierKey))
            {
                transaction.TransactionIdentifier = dict[TransactionIdentifierKey].ToString();
            }

            if(dict.ContainsKey(Base64EncodedReceiptKey))
            {
                transaction.Base64EncodedTransactionReceipt = dict[Base64EncodedReceiptKey].ToString();
            }

            if(dict.ContainsKey(TransactionStateKey))
            {
                transaction.TransactionState = (IosStoreTransactionState)int.Parse(dict[TransactionStateKey].ToString());
            }

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
