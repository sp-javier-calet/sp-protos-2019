using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class AndroidStoreProduct
    {
        public string ItemType { get; private set; }

        public string Sku { get; private set; }

        public string Type { get; private set; }

        public string Price { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string CurrencyCode { get; private set; }

        public string PriceValue { get; private set; }


        public static List<AndroidStoreProduct> ProductsFromJson(string json)
        {
            return AndroidStoreAttrUtils.AndroidStoreListFromJson<AndroidStoreProduct>(json, ProductFromDictionary);
        }

        public static AndroidStoreProduct TransactionFromJson(string json)
        {
            return AndroidStoreAttrUtils.AndroidStoreObjectFromJson<AndroidStoreProduct>(json, ProductFromDictionary);
        }

        public static AndroidStoreProduct ProductFromDictionary(AttrDic data)
        {
            AndroidStoreProduct product = new AndroidStoreProduct();

            if(data.ContainsKey("itemType"))
                product.ItemType = data["itemType"].ToString();

            if(data.ContainsKey("sku"))
                product.Sku = data["sku"].ToString();

            if(data.ContainsKey("type"))
                product.Type = data["type"].ToString();

            if(data.ContainsKey("price"))
                product.Price = data["price"].ToString();

            if(data.ContainsKey("title"))
                product.Title = data["title"].ToString();

            if(data.ContainsKey("description"))
                product.Description = data["description"].ToString();

            if(data.ContainsKey("currencyCode"))
                product.CurrencyCode = data["currencyCode"].ToString();

            if(data.ContainsKey("priceValue"))
                product.PriceValue = data["priceValue"].ToString();

            return product;
        }


        public override string ToString()
        {
            return string.Format("[SkuDetails: type = {0}, SKU = {1}, title = {2}, price = {3}, description = {4}, priceValue={5}, currency={6}]",
                ItemType, Sku, Title, Price, Description, PriceValue, CurrencyCode);
        }
    }
}