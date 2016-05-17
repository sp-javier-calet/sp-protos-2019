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

        private const string ProductIdentifierKey = "productIdentifier";
        private const string LocalizedTitleKey = "localizedTitle";
        private const string LocalizedDescriptionKey = "localizedDescription";
        private const string PriceKey = "price";
        private const string CurrencySymbolKey = "currencySymbol";
        private const string FormattedPriceKey = "formattedPrice";


        public static List<IosStoreProduct> ProductsFromJson(string json)
        {
            return IosStoreAttrUtils.IosStoreListFromJson<IosStoreProduct>(json, ProductFromDictionary);
        }


        public static IosStoreProduct ProductFromDictionary(AttrDic data)
        {
            IosStoreProduct product = new IosStoreProduct();

            if(data.ContainsKey(ProductIdentifierKey))
            {
                product.ProductIdentifier = data[ProductIdentifierKey].ToString();
            }

            if(data.ContainsKey(LocalizedTitleKey))
            {
                product.Title = data[LocalizedTitleKey].ToString();
            }

            if(data.ContainsKey(LocalizedDescriptionKey))
            {
                product.Description = data[LocalizedDescriptionKey].ToString();
            }

            if(data.ContainsKey(PriceKey))
            {
                product.Price = data[PriceKey].ToString();
            }

            if(data.ContainsKey(CurrencySymbolKey))
            {
                product.CurrencySymbol = data[CurrencySymbolKey].ToString();
            }
            
            if(data.ContainsKey(FormattedPriceKey))
            {
                product.FormattedPrice = data[FormattedPriceKey].ToString();
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
