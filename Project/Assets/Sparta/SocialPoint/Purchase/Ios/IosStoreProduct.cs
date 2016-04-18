using System;
using System.Collections;
using System.Collections.Generic;

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

        public string CurrencyCode { get; private set; }

        public string FormattedPrice { get; private set; }

        string _countryCode;
        string _downloadContentVersion;
        bool _downloadable;
        List<Int64> _downloadContentLengths = new List<Int64>();


        public static List<IosStoreProduct> ProductsFromJson(string json)
        {
            var productList = new List<IosStoreProduct>();

            //UPDATE NEEDED!
            List<object> products = null;//json.listFromJson();
            foreach(Dictionary<string, object> ht in products)
                productList.Add(ProductFromDictionary(ht));

            return productList;
        }


        public static IosStoreProduct ProductFromDictionary(Dictionary<string,object> ht)
        {
            IosStoreProduct product = new IosStoreProduct();

            if(ht.ContainsKey("productIdentifier"))
                product.ProductIdentifier = ht["productIdentifier"].ToString();

            if(ht.ContainsKey("localizedTitle"))
                product.Title = ht["localizedTitle"].ToString();

            if(ht.ContainsKey("localizedDescription"))
                product.Description = ht["localizedDescription"].ToString();

            if(ht.ContainsKey("price"))
                product.Price = ht["price"].ToString();

            if(ht.ContainsKey("currencySymbol"))
                product.CurrencySymbol = ht["currencySymbol"].ToString();

            if(ht.ContainsKey("currencyCode"))
                product.CurrencyCode = ht["currencyCode"].ToString();

            if(ht.ContainsKey("formattedPrice"))
                product.FormattedPrice = ht["formattedPrice"].ToString();

            if(ht.ContainsKey("countryCode"))
                product._countryCode = ht["countryCode"].ToString();

            if(ht.ContainsKey("downloadContentVersion"))
                product._downloadContentVersion = ht["downloadContentVersion"].ToString();

            if(ht.ContainsKey("downloadable"))
                product._downloadable = bool.Parse(ht["downloadable"].ToString());

            if(ht.ContainsKey("downloadContentLengths") && ht["downloadContentLengths"] is IList)
            {
                var tempLengths = ht["downloadContentLengths"] as List<object>;
                foreach(var dlLength in tempLengths)
                    product._downloadContentLengths.Add(System.Convert.ToInt64(dlLength));
            }

            return product;
        }


        public override string ToString()
        {
            return String.Format("<IosStoreProduct>\nID: {0}\ntitle: {1}\ndescription: {2}\nprice: {3}\ncurrencysymbol: {4}\nformattedPrice: {5}\ncurrencyCode: {6}\ncountryCode: {7}\ndownloadContentVersion: {8}\ndownloadable: {9}",
                ProductIdentifier, Title, Description, Price, CurrencySymbol, FormattedPrice, CurrencyCode, _countryCode, _downloadContentVersion, _downloadable);
        }

    }
}
#endif
