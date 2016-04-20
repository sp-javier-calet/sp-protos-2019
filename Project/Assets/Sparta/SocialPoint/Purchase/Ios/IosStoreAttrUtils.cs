using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public class IosStoreAttrUtils
    {
        public delegate T IosStoreObjectFromAttrDic<T>(AttrDic dict);

        public static List<T> IosStoreListFromJson<T>(string json, IosStoreObjectFromAttrDic<T> ObjectBuilder)
        {
            List<T> productList = new List<T>();

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
                        productList.Add(ObjectBuilder(pData.AsDic));
                    }
                }
            }

            return productList;
        }

        public static T IosStoreObjectFromJson<T>(string json, IosStoreObjectFromAttrDic<T> ObjectBuilder)
        {
            LitJsonAttrParser litJsonParser = new LitJsonAttrParser();
            Attr parsedData = litJsonParser.ParseString(json);
            AttrDic dict = null;
            if(parsedData.AttrType == AttrType.DICTIONARY)
            {
                dict = parsedData.AsDic;
            }

            if(dict == null)
                return default(T);

            return ObjectBuilder(dict);
        }
    }
}
#endif
