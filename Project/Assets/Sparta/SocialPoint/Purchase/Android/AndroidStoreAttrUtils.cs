﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class AndroidStoreAttrUtils
    {
        public delegate T AndroidStoreObjectFromAttrDic<T>(AttrDic dict);

        public static List<T> AndroidStoreListFromJson<T>(string json, AndroidStoreObjectFromAttrDic<T> ObjectBuilder)
        {
            List<T> productList = new List<T>();

            JsonAttrParser jsonParser = new JsonAttrParser();
            Attr parsedData = jsonParser.ParseString(json);
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

        public static T AndroidStoreObjectFromJson<T>(string json, AndroidStoreObjectFromAttrDic<T> ObjectBuilder)
        {
            JsonAttrParser litJsonParser = new JsonAttrParser();
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
