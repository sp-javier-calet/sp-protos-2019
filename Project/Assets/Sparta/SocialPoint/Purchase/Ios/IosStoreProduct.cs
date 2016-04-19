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

        public static List<IosStoreProduct> ProductsFromJson(string json)
        {
            var productList = new List<IosStoreProduct>();

            LitJsonAttrParser litJsonParser = new LitJsonAttrParser();
            Attr parsedData = litJsonParser.ParseString(json);
            if(parsedData.AttrType == AttrType.LIST)
            {
                AttrList products = parsedData.AsList;
                for(int i = 0; i < products.Count; ++i)
                {
                    Attr pData = products[i];
                    if(pData.AttrType == AttrType.DICTIONARY)
                    {
                        productList.Add(ProductFromDictionary(pData.AsDic));
                    }
                }
            }

            return productList;
        }


        public static IosStoreProduct ProductFromDictionary(AttrDic data)
        {
            IosStoreProduct product = new IosStoreProduct();

            if(data.ContainsKey("productIdentifier"))
                product.ProductIdentifier = data["productIdentifier"].ToString();

            if(data.ContainsKey("localizedTitle"))
                product.Title = data["localizedTitle"].ToString();

            if(data.ContainsKey("localizedDescription"))
                product.Description = data["localizedDescription"].ToString();

            if(data.ContainsKey("price"))
                product.Price = data["price"].ToString();

            if(data.ContainsKey("currencySymbol"))
                product.CurrencySymbol = data["currencySymbol"].ToString();
            
            if(data.ContainsKey("formattedPrice"))
                product.FormattedPrice = data["formattedPrice"].ToString();

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
