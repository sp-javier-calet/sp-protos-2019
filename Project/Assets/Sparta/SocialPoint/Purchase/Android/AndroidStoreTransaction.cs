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

        private const string _itemTypeKey = "itemType";
        private const string _orderIdKey = "orderId";
        private const string _packageNameKey = "packageName";
        private const string _skuKey = "sku";
        private const string _purchaseTimeKey = "purchaseTime";
        private const string _purchaseStateKey = "purchaseState";
        private const string _developerPayloadKey = "developerPayload";
        private const string _tokenKey = "token";
        private const string _originalJsonKey = "originalJson";
        private const string _signatureKey = "signature";


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

            if(dict.ContainsKey(_itemTypeKey))
            {
                transaction.ItemType = dict[_itemTypeKey].ToString();
            }

            if(dict.ContainsKey(_orderIdKey))
            {
                transaction.OrderId = dict[_orderIdKey].ToString();
            }

            if(dict.ContainsKey(_packageNameKey))
            {
                transaction.PackageName = dict[_packageNameKey].ToString();
            }

            if(dict.ContainsKey(_skuKey))
            {
                transaction.Sku = dict[_skuKey].ToString();
            }

            if(dict.ContainsKey(_purchaseTimeKey) && dict[_purchaseTimeKey].IsValue)
            {
                transaction.PurchaseTime = dict[_purchaseTimeKey].AsValue.ToLong();
            }

            if(dict.ContainsKey(_purchaseStateKey) && dict[_purchaseStateKey].IsValue)
            {
                transaction.PurchaseState = dict[_purchaseStateKey].AsValue.ToInt();
            }

            if(dict.ContainsKey(_developerPayloadKey))
            {
                transaction.DeveloperPayload = dict[_developerPayloadKey].ToString();
            }

            if(dict.ContainsKey(_tokenKey))
            {
                transaction.Token = dict[_tokenKey].ToString();
            }

            if(dict.ContainsKey(_originalJsonKey))
            {
                transaction.OriginalJson = dict[_originalJsonKey].ToString();
            }
            
            if(dict.ContainsKey(_signatureKey))
            {
                transaction.Signature = dict[_signatureKey].ToString();
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