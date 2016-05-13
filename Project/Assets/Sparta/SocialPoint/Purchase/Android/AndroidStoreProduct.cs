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

        private const string _itemTypeKey = "itemType";
        private const string _skuKey = "sku";
        private const string _typeKey = "type";
        private const string _priceKey = "price";
        private const string _titleKey = "title";
        private const string _descriptionKey = "description";
        private const string _currencyCodeKey = "currencyCode";
        private const string _priceValueKey = "priceValue";


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

            if(data.ContainsKey(_itemTypeKey))
            {
                product.ItemType = data[_itemTypeKey].ToString();
            }

            if(data.ContainsKey(_skuKey))
            {
                product.Sku = data[_skuKey].ToString();
            }

            if(data.ContainsKey(_typeKey))
            {
                product.Type = data[_typeKey].ToString();
            }

            if(data.ContainsKey(_priceKey))
            {
                product.Price = data[_priceKey].ToString();
            }

            if(data.ContainsKey(_titleKey))
            {
                product.Title = data[_titleKey].ToString();
            }

            if(data.ContainsKey(_descriptionKey))
            {
                product.Description = data[_descriptionKey].ToString();
            }

            if(data.ContainsKey(_currencyCodeKey))
            {
                product.CurrencyCode = data[_currencyCodeKey].ToString();
            }

            if(data.ContainsKey(_priceValueKey))
            {
                try
                {
                    float value = float.Parse(data[_priceValueKey].ToString());
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