using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocialPoint.Attributes;

#if UNITY_ANDROID
namespace SocialPoint.Purchase
{
    public class AndroidStoreTransaction
    {
        /// <summary>
        /// ITEM_TYPE_INAPP or ITEM_TYPE_SUBS
        /// </summary>
        public string ItemType { get; private set; }

        /// <summary>
        /// A unique order identifier for the transaction. This corresponds to the Google Wallet Order ID.
        /// </summary>
        public string OrderId { get; private set; }

        /// <summary>
        /// The application package from which the purchase originated.
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// The item's product identifier. Every item has a product ID, which you must specify in the application's product list on the Google Play Developer Console.
        /// </summary>
        public string Sku { get; private set; }

        /// <summary>
        /// The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
        /// </summary>
        public long PurchaseTime { get; private set; }

        /// <summary>
        /// The purchase state of the order. Possible values are 0 (purchased), 1 (canceled), or 2 (refunded).
        /// </summary>
        public int PurchaseState { get; private set; }

        /// <summary>
        /// A developer-specified string that contains supplemental information about an order. You can specify a value for this field when you make a getBuyIntent request.
        /// </summary>
        public string DeveloperPayload { get; private set; }

        /// <summary>
        /// A token that uniquely identifies a purchase for a given item and user pair. 
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// JSON sent by the current store
        /// </summary>
        public string OriginalJson { get; private set; }

        /// <summary>
        /// Signature of the JSON string
        /// </summary>
        public string Signature { get; private set; }

        private const string ItemTypeKey = "itemType";
        private const string OrderIdKey = "orderId";
        private const string PackageNameKey = "packageName";
        private const string SkuKey = "sku";
        private const string PurchaseTimeKey = "purchaseTime";
        private const string PurchaseStateKey = "purchaseState";
        private const string DeveloperPayloadKey = "developerPayload";
        private const string TokenKey = "token";
        private const string OriginalJsonKey = "originalJson";
        private const string SignatureKey = "signature";


        public static List<AndroidStoreTransaction> TransactionsFromJson(string json)
        {
            return AndroidStoreAttrUtils.AndroidStoreListFromJson<AndroidStoreTransaction>(json, TransactionFromDictionary);
        }


        public static AndroidStoreTransaction TransactionFromJson(string json)
        {
            return AndroidStoreAttrUtils.AndroidStoreObjectFromJson<AndroidStoreTransaction>(json, TransactionFromDictionary);
        }


        public static AndroidStoreTransaction TransactionFromDictionary(AttrDic dict)
        {
            AndroidStoreTransaction transaction = new AndroidStoreTransaction();

            if(dict.ContainsKey(ItemTypeKey))
            {
                transaction.ItemType = dict[ItemTypeKey].ToString();
            }

            if(dict.ContainsKey(OrderIdKey))
            {
                transaction.OrderId = dict[OrderIdKey].ToString();
            }

            if(dict.ContainsKey(PackageNameKey))
            {
                transaction.PackageName = dict[PackageNameKey].ToString();
            }

            if(dict.ContainsKey(SkuKey))
            {
                transaction.Sku = dict[SkuKey].ToString();
            }

            if(dict.ContainsKey(PurchaseTimeKey) && dict[PurchaseTimeKey].IsValue)
            {
                transaction.PurchaseTime = dict[PurchaseTimeKey].AsValue.ToLong();
            }

            if(dict.ContainsKey(PurchaseStateKey) && dict[PurchaseStateKey].IsValue)
            {
                transaction.PurchaseState = dict[PurchaseStateKey].AsValue.ToInt();
            }

            if(dict.ContainsKey(DeveloperPayloadKey))
            {
                transaction.DeveloperPayload = dict[DeveloperPayloadKey].ToString();
            }

            if(dict.ContainsKey(TokenKey))
            {
                transaction.Token = dict[TokenKey].ToString();
            }

            if(dict.ContainsKey(OriginalJsonKey))
            {
                transaction.OriginalJson = dict[OriginalJsonKey].ToString();
            }
            
            if(dict.ContainsKey(SignatureKey))
            {
                transaction.Signature = dict[SignatureKey].ToString();
            }

            return transaction;
        }


        public static AndroidStoreTransaction CreateFromSku(string sku)
        {
            return CreateFromSku(sku, "");
        }

        public static AndroidStoreTransaction CreateFromSku(string sku, string developerPayload)
        {
            AndroidStoreTransaction p = new AndroidStoreTransaction();
            p.Sku = sku;
            p.DeveloperPayload = developerPayload;

            return p;
        }


        public override string ToString()
        {
            return string.Format("[AndroidStoreTransaction: SKU = {0}, OriginalJson = {1}", Sku, OriginalJson);
        }
    }
}
#endif