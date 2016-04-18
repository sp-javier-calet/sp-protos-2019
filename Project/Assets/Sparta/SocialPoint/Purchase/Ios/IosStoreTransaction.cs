using System;
using System.Collections;
using System.Collections.Generic;

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
        Deferreds
    }

    public class IosStoreTransaction
    {
        public string ProductIdentifier { get; private set; }

        public string TransactionIdentifier { get; private set; }

        public string Base64EncodedTransactionReceipt { get; private set; }

        public int Quantity { get; private set; }

        public int Downloads { get; private set; }

        public IosStoreTransactionState TransactionState { get; private set; }



        public static List<IosStoreTransaction> TransactionsFromJson(string json)
        {
            var transactionList = new List<IosStoreTransaction>();

            //UPDATE NEEDED!
            List<object> transactions = null;//json.listFromJson();
            if(transactions == null)
                return transactionList;

            foreach(Dictionary<string, object> dict in transactions)
                transactionList.Add(TransactionFromDictionary(dict));

            return transactionList;
        }


        public static IosStoreTransaction TransactionFromJson(string json)
        {
            //UPDATE NEEDED!
            Dictionary<string, object> dict = null;//json.dictionaryFromJson();

            if(dict == null)
                return new IosStoreTransaction();

            return TransactionFromDictionary(dict);
        }


        public static IosStoreTransaction TransactionFromDictionary(Dictionary<string, object> dict)
        {
            var transaction = new IosStoreTransaction();

            if(dict.ContainsKey("productIdentifier"))
                transaction.ProductIdentifier = dict["productIdentifier"].ToString();

            if(dict.ContainsKey("transactionIdentifier"))
                transaction.TransactionIdentifier = dict["transactionIdentifier"].ToString();

            if(dict.ContainsKey("base64EncodedReceipt"))
                transaction.Base64EncodedTransactionReceipt = dict["base64EncodedReceipt"].ToString();

            if(dict.ContainsKey("quantity"))
                transaction.Quantity = int.Parse(dict["quantity"].ToString());

            if(dict.ContainsKey("transactionState"))
                transaction.TransactionState = (IosStoreTransactionState)int.Parse(dict["transactionState"].ToString());

            if(dict.ContainsKey("downloads"))
                transaction.Downloads = int.Parse(dict["downloads"].ToString());

            return transaction;
        }


        public override string ToString()
        {
            return string.Format("<IosStoreTransaction> ID: {0}, quantity: {1}, transactionIdentifier: {2}, transactionState: {3}, downloads: {4}",
                ProductIdentifier, Quantity, TransactionIdentifier, TransactionState, Downloads);
        }

    }
}
#endif
