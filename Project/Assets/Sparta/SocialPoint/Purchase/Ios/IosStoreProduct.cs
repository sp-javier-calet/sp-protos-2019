using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public class IosStoreProduct
    {
        public string ProductIdentifier { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Price { get; private set; }

        public string CurrencySymbol { get; private set; }

        public string FormattedPrice { get; private set; }

        private const string _productIdentifierKey = "productIdentifier";
        private const string _localizedTitleKey = "localizedTitle";
        private const string _localizedDescriptionKey = "localizedDescription";
        private const string _priceKey = "price";
        private const string _currencySymbolKey = "currencySymbol";
        private const string _formattedPriceKey = "formattedPrice";


        public static List<IosStoreProduct> ProductsFromJson(string json)
        {
            return IosStoreAttrUtils.IosStoreListFromJson<IosStoreProduct>(json, ProductFromDictionary);
        }


        public static IosStoreProduct ProductFromDictionary(AttrDic data)
        {
            IosStoreProduct product = new IosStoreProduct();

            if(data.ContainsKey(_productIdentifierKey))
            {
                product.ProductIdentifier = data[_productIdentifierKey].ToString();
            }

            if(data.ContainsKey(_localizedTitleKey))
            {
                product.Title = data[_localizedTitleKey].ToString();
            }

            if(data.ContainsKey(_localizedDescriptionKey))
            {
                product.Description = data[_localizedDescriptionKey].ToString();
            }

            if(data.ContainsKey(_priceKey))
            {
                product.Price = data[_priceKey].ToString();
            }

            if(data.ContainsKey(_currencySymbolKey))
            {
                product.CurrencySymbol = data[_currencySymbolKey].ToString();
            }
            
            if(data.ContainsKey(_formattedPriceKey))
            {
                product.FormattedPrice = data[_formattedPriceKey].ToString();
            }

            return product;
        }


        public override string ToString()
        {
            return String.Format("<IosStoreProduct>\nID: {0}\ntitle: {1}\ndescription: {2}\nprice: {3}\ncurrencysymbol: {4}\nformattedPrice: {5}",
                ProductIdentifier, Title, Description, Price, CurrencySymbol, FormattedPrice);
        }

    }
}
#endif
