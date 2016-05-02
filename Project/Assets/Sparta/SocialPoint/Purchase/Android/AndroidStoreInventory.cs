using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Purchase
{
    public class AndroidStoreInventory
    {
        private Dictionary<string, AndroidStoreProduct> _skuMap = new Dictionary<string, AndroidStoreProduct>();
        private Dictionary<string, AndroidStoreTransaction> _purchaseMap = new Dictionary<string, AndroidStoreTransaction>();

        public AndroidStoreInventory(string json)
        {
            //TODO: Parse all AttrDic
            /*var j = new JSON(json);
            foreach(var entry in (List<object>) j.fields["purchaseMap"])
            {
                List<object> pair = (List<object>)entry;
                string key = pair[0].ToString();
                AndroidStoreTransaction value = new AndroidStoreTransaction(pair[1].ToString());
                _purchaseMap.Add(key, value);
            }
            foreach(var entry in (List<object>) j.fields["skuMap"])
            {
                List<object> pair = (List<object>)entry;
                string key = pair[0].ToString();
                AndroidStoreProduct value = new AndroidStoreProduct(pair[1].ToString());
                _skuMap.Add(key, value);
            }*/
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("{purchaseMap:{");
            foreach(var pair in _purchaseMap)
            {
                str.Append("\"" + pair.Key + "\":{" + pair.Value.ToString() + "},");
            }
            str.Append("},");
            str.Append("skuMap:{");
            foreach(var pair in _skuMap)
            {
                str.Append("\"" + pair.Key + "\":{" + pair.Value.ToString() + "},");
            }
            str.Append("}}");
            return str.ToString();
        }

        /**
         * Returns the listing details for an in-app product.
         */
        public AndroidStoreProduct GetAndroidStoreProduct(string sku)
        {
            if(!_skuMap.ContainsKey(sku))
            {
                return null;
            }
            return _skuMap[sku];
        }

        /**
         * Returns purchase information for a given product, or null if there is no purchase.
         */
        public AndroidStoreTransaction GetPurchase(string sku)
        {
            if(!_purchaseMap.ContainsKey(sku))
            {
                return null;
            }
            return _purchaseMap[sku];
        }

        /**
         * Returns whether or not there exists a purchase of the given product.
         */
        public bool HasPurchase(string sku)
        {
            return _purchaseMap.ContainsKey(sku);
        }

        /**
         * Return whether or not details about the given product are available.
         */
        public bool HasDetails(string sku)
        {
            return _skuMap.ContainsKey(sku);
        }

        /**
         * Erase a purchase (locally) from the inventory, given its product ID. This just
         * modifies the Inventory object locally and has no effect on the server! This is
         * useful when you have an existing Inventory object which you know to be up to date,
         * and you have just consumed an item successfully, which means that erasing its
         * purchase data from the Inventory you already have is quicker than querying for
         * a new Inventory.
         */
        public void ErasePurchase(string sku)
        {
            if(_purchaseMap.ContainsKey(sku))
                _purchaseMap.Remove(sku);
        }

        /**
         * Returns a list of all owned product IDs.
         */
        public List<string> GetAllOwnedSkus()
        {
            return _purchaseMap.Keys.ToList<string>();
        }

        /**
         * Returns a list of all owned product IDs of a given type
         */
        public List<string> GetAllOwnedSkus(string itemType)
        {
            List<string> result = new List<string>();
            foreach(AndroidStoreTransaction p in _purchaseMap.Values)
            {
                if(p.ItemType == itemType)
                    result.Add(p.Sku);
            }
            return result;
        }

        /**
         * Returns a list of all purchases.
         */
        public List<AndroidStoreTransaction> GetAllPurchases()
        {
            return _purchaseMap.Values.ToList<AndroidStoreTransaction>();
        }

        /** 
         * Returns a list of all available {@code AndroidStoreProduct} products. 
         */
        public List<AndroidStoreProduct> GetAllAvailableSkus()
        {
            return _skuMap.Values.ToList();
        }

        public void AddAndroidStoreProduct(AndroidStoreProduct d)
        {
            _skuMap.Add(d.Sku, d);
        }

        public void AddPurchase(AndroidStoreTransaction p)
        {
            _purchaseMap.Add(p.Sku, p);
        }
    }
}
