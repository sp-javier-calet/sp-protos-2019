using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

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

            if(dict.ContainsKey("itemType"))
                transaction.ItemType = dict["itemType"].ToString();

            if(dict.ContainsKey("orderId"))
                transaction.OrderId = dict["orderId"].ToString();

            if(dict.ContainsKey("packageName"))
                transaction.PackageName = dict["packageName"].ToString();

            if(dict.ContainsKey("sku"))
                transaction.Sku = dict["sku"].ToString();

            if(dict.ContainsKey("purchaseTime") && dict["purchaseTime"].IsValue)
                transaction.PurchaseTime = dict["purchaseTime"].AsValue.ToLong();

            if(dict.ContainsKey("purchaseState") && dict["purchaseState"].IsValue)
                transaction.PurchaseState = dict["purchaseState"].AsValue.ToInt();

            if(dict.ContainsKey("developerPayload"))
                transaction.DeveloperPayload = dict["developerPayload"].ToString();

            if(dict.ContainsKey("token"))
                transaction.Token = dict["token"].ToString();

            if(dict.ContainsKey("originalJson"))
                transaction.OriginalJson = dict["originalJson"].ToString();
            
            if(dict.ContainsKey("signature"))
                transaction.Signature = dict["signature"].ToString();

            UnityEngine.Debug.Log("*** TEST Transaction Loaded: " + transaction.ToString());
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
            return "SKU:" + Sku + ";" + OriginalJson;
        }


        /**
         * Serilize to json
         * @return json string
         */ 
        public string Serialize()
        {
            AttrDic dic = new AttrDic();
            dic.SetValue("itemType", ItemType);
            dic.SetValue("orderId", OrderId);
            dic.SetValue("packageName", PackageName);
            dic.SetValue("sku", Sku);
            dic.SetValue("purchaseTime", PurchaseTime);
            dic.SetValue("purchaseState", PurchaseState);
            dic.SetValue("developerPayload", DeveloperPayload);
            dic.SetValue("token", Token);
            dic.SetValue("originalJson", OriginalJson);
            dic.SetValue("signature", Signature);
            return dic.ToString();
        }
    }
}