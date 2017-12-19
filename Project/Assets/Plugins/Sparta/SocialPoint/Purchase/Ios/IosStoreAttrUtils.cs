using System.Collections.Generic;
using SocialPoint.Attributes;

#if (UNITY_IOS || UNITY_TVOS)
namespace SocialPoint.Purchase
{
    public sealed class IosStoreAttrUtils
    {
        public delegate T IosStoreObjectFromAttrDic<T>(AttrDic dict);

        public static List<T> IosStoreListFromJson<T>(string json, IosStoreObjectFromAttrDic<T> ObjectBuilder)
        {
            var productList = new List<T>();

            var litJsonParser = new JsonAttrParser();
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
            var litJsonParser = new JsonAttrParser();
            Attr parsedData = litJsonParser.ParseString(json);
            AttrDic dict = null;
            if(parsedData.AttrType == AttrType.DICTIONARY)
            {
                dict = parsedData.AsDic;
            }

            return dict == null ? default(T) : ObjectBuilder(dict);

        }
    }
}
#endif
