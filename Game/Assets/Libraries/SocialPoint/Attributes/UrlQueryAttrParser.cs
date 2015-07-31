using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using System.Linq;

namespace SocialPoint.Attributes
{
    public class UrlQueryAttrParser : IAttrParser
    {
        private const string TokenStart = "?";
        private const string TokenSeparator = "&";
        private const string TokenAssign = "=";
        private const string TokenGroupStart = "[";
        private const string TokenGroupEnd = "]";
        
        public UrlQueryAttrParser()
        {
        }
        
        List<string> SplitName(string name)
        {
            List<string> iparts = name.Split(TokenGroupStart.ToCharArray()).ToList();
            List<string> parts = new List<string>();
            var enumList = iparts.GetEnumerator();
            while(enumList.MoveNext())
            {
                parts.Add(Uri.UnescapeDataString(enumList.Current).Trim(TokenGroupEnd.ToCharArray()));
            }
            return parts;
        }
        
        Attr CreateAttr(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return new AttrList();
            }
            else
            {
                return new AttrDic();
            }
        }
        
        bool SetAttr(ref Attr root, List<string> name, Attr value)
        {
            Attr childAttr = null;
            List<string> childName = new List<string>(name);
            
            if(childName.Count == 0)
            {
                return false;
            }
            
            string ppart = childName.Last();
            childName.RemoveAt((childName.Count - 1));
            
            if(childName.Count == 0)
            {
                if(string.IsNullOrEmpty(ppart))
                {
                    root.AsList.Add(value);
                }
                else
                {
                    root.AsDic.Set(ppart, value);
                }
                return true;
            }
            
            string part = childName.Last();
            
            if(string.IsNullOrEmpty(ppart))
            {
                AttrList currList = root.AsList;
                int s = currList.Count;
                if(s > 0)
                {
                    childAttr = currList.Get(s - 1);
                }
                else
                {
                    childAttr = CreateAttr(part);
                    currList.Add(childAttr);
                }
                return SetAttr(ref childAttr, childName, value);
            }
            else
            {
                AttrDic currDict = root.AsDic;
                if(currDict.ContainsKey(ppart))
                {
                    childAttr = currDict.Get(ppart);
                }
                else
                {
                    childAttr = CreateAttr(part);
                    currDict.Set(ppart, childAttr);
                }
                return SetAttr(ref childAttr, childName, value);
            }
        }
        
        public Attr Parse(Data data)
        {
            Attr root = new AttrDic();
            string str = data.String.TrimStart(TokenStart.ToCharArray());
            List<string> tokens = str.Split(TokenSeparator.ToCharArray()).ToList();
            var enumList = tokens.GetEnumerator();
            while(enumList.MoveNext())
            {
                var token = enumList.Current.Trim();
                if(token.Length > 0)
                {
                    List<string> parts = token.Split(TokenAssign.ToCharArray()).ToList();
                    if(parts.Count < 2)
                    {
                        AttrDic rootDic = root.AsDic;
                        if(!rootDic.ContainsKey(""))
                        {
                            rootDic.Set("", new AttrList());
                        }
                        rootDic.Get("").AsList.AddValue(Uri.UnescapeDataString(parts[0]));
                    }
                    else
                    {
                        List<string> name = SplitName(parts[0]);
                        SetAttr(ref root, name, new AttrString(Uri.UnescapeDataString(parts[1])));
                    }
                }
            }

            return root;
        }

    }
}