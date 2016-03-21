
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public class UrlQueryAttrSerializer
    {
        private const string TokenSeparator = "&";
        private const string TokenAssign = "=";
        private const string TokenGroupStart = "[";
        private const string TokenGroupEnd = "]";

        public UrlQueryAttrSerializer()
        {
        }
        
        private string Convert(Attr data, string prefix)
        {
            var str = new StringBuilder();
            bool first = true;
            if(data == null)
            {
                data = new AttrEmpty();
            }
            switch(data.AttrType)
            {
            case AttrType.VALUE:
                var dataval = data.AsValue;
                if(!string.IsNullOrEmpty(prefix))
                {
                    str.Append(prefix);
                    str.Append(TokenAssign);
                }
                if(dataval.AttrValueType != AttrValueType.EMPTY)
                {
                    str.Append(Uri.EscapeDataString(dataval.ToString()));
                }
                break;
            case AttrType.DICTIONARY:
                first = true;
                var itrDic = data.AsDic.GetEnumerator();
                while(itrDic.MoveNext())
                {
                    var item = itrDic.Current;
                    if(first)
                    {
                        first = false;
                    }
                    else
                    {
                        str.Append(TokenSeparator);
                    }
                    string newprefix = Uri.EscapeDataString(item.Key);
                    if(!string.IsNullOrEmpty(prefix))
                    {
                        newprefix = prefix + TokenGroupStart + newprefix + TokenGroupEnd;
                    }
                    str.Append(Convert(item.Value, newprefix));
                }
                break;
            case AttrType.LIST:
                first = true;
                var itrList = data.AsList.GetEnumerator();
                while(itrList.MoveNext())
                {
                    Attr item = itrList.Current;
                    if(first)
                    {
                        first = false;
                    }
                    else
                    {
                        str.Append(TokenSeparator);
                    }
                    str.Append(Convert(item, string.IsNullOrEmpty(prefix) ? prefix : prefix + TokenGroupStart + TokenGroupEnd));
                }
                break;
            default:
                break;
            }
            return str.ToString();
        }

        public byte[] Serialize(Attr attr)
        {
            return Encoding.UTF8.GetBytes(SerializeString(attr));
        }

        public string SerializeString(Attr attr)
        {
            return Convert(attr, string.Empty);
        }
    }
}

