using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

#if UNITY_ANDROID
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

        private const string ItemTypeKey = "itemType";
        private const string SkuKey = "sku";
        private const string TypeKey = "type";
        private const string PriceKey = "price";
        private const string TitleKey = "title";
        private const string DescriptionKey = "description";
        private const string CurrencyCodeKey = "currencyCode";
        private const string PriceValueKey = "priceValue";


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

            if(data.ContainsKey(ItemTypeKey))
            {
                product.ItemType = data[ItemTypeKey].ToString();
            }

            if(data.ContainsKey(SkuKey))
            {
                product.Sku = data[SkuKey].ToString();
            }

            if(data.ContainsKey(TypeKey))
            {
                product.Type = data[TypeKey].ToString();
            }

            if(data.ContainsKey(PriceKey))
            {
                product.Price = data[PriceKey].ToString();
            }

            if(data.ContainsKey(TitleKey))
            {
                product.Title = data[TitleKey].ToString();
            }

            if(data.ContainsKey(DescriptionKey))
            {
                product.Description = data[DescriptionKey].ToString();
            }

            if(data.ContainsKey(CurrencyCodeKey))
            {
                product.CurrencyCode = data[CurrencyCodeKey].ToString();
            }

            if(data.ContainsKey(PriceValueKey))
            {
                try
                {
                    float value = float.Parse(data[PriceValueKey].ToString());
                    value /= 1000000;//Prices in store are stored x1.000.000
                    product.PriceValue = value.ToString();
                }
                catch
                {
                    product.PriceValue = "";
                }
            }

            return product;
        }


        public override string ToString()
        {
            return string.Format("[AndroidStoreProduct: type = {0}, SKU = {1}, title = {2}, price = {3}, description = {4}, priceValue={5}, currency={6}]",
                ItemType, Sku, Title, Price, Description, PriceValue, CurrencyCode);
        }
    }
}
#endif