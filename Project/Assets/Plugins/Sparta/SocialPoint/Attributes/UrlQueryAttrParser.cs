using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Attributes
{
    public sealed class UrlQueryAttrParser : IAttrParser
    {
        private const char TokenStart = '?';
        private const char TokenSeparator = '&';
        private const char TokenAssign = '=';
        private const char TokenGroupStart = '[';
        private const char TokenGroupEnd = ']';
        
        public UrlQueryAttrParser()
        {
        }
        
        string[] SplitName(string name)
        {
            var parts = name.Split(new char[]{ TokenGroupStart });
            for(var i = 0; i < parts.Length; i++)
            {
                parts[i] = Uri.UnescapeDataString(parts[i]).Trim(new char[]{ TokenGroupEnd });
            }
            return parts;
        }
        
        Attr CreateAttr(string name)
        {
            int i;
            if(string.IsNullOrEmpty(name) || int.TryParse(name, out i))
            {
                return new AttrList();
            }
            else
            {
                return new AttrDic();
            }
        }
        
        void SetAttr(Attr attr, string[] name, Attr value)
        {
            Attr fix = null;
            var parents = new Attr[name.Length];
            for(var i = 0; i < name.Length; i++)
            {
                parents[i] = attr;
                var last = i >= name.Length - 1;
                var part = name[i];
                var type = attr.AttrType;
                if(type == AttrType.DICTIONARY)
                {
                    var dic = attr.AsDic;
                    if(last)
                    {
                        dic.Set(part, value);    
                    }
                    else if(dic.ContainsKey(part))
                    {
                        attr = dic.Get(part);
                    }
                    else
                    {
                        attr = CreateAttr(name[i + 1]);
                        dic.Set(part, attr);
                    }
                }
                else if(type == AttrType.LIST)
                {
                    var list = attr.AsList;
                    int ipart = 0;

                    if(string.IsNullOrEmpty(part))
                    {
                        ipart = list.Count;
                    }
                    else if(!int.TryParse(part, out ipart))
                    {
                        // should be a dictionary
                        fix = new AttrDic(list);
                    }
                    if(fix == null)
                    {
                        bool exists = list.Count > ipart;
                        while(list.Count <= ipart)
                        {
                            list.Add(new AttrString());
                        }
                        if(last)
                        {
                            list.Set(ipart, value);
                        }
                        else if(exists)
                        {
                            attr = list.Get(ipart);
                        }
                        else
                        {
                            attr = CreateAttr(name[i + 1]);
                            list.Set(ipart, attr);
                        }
                    }
                }
                else
                {
                    // should be a list or dictionary
                    fix = CreateAttr(part);
                }
                if(i > 0 && fix != null)
                {
                    i--;
                    var parent = parents[i];
                    part = name[i];
                    type = parent.AttrType;
                    if(type == AttrType.DICTIONARY)
                    {
                        parent.AsDic.Set(part, fix);
                    }
                    else if(type == AttrType.LIST)
                    {
                        var list = parent.AsList;
                        var ipart = 0;
                        if(string.IsNullOrEmpty(part))
                        {
                            ipart = list.Count - 1;
                        }
                        else
                        {
                            int.TryParse(part, out ipart);
                        }
                        list.Set(ipart, fix);
                    }
                    attr = fix;
                    fix = null;
                }
            }
        }

        public Attr Parse(byte[] data)
        {
            return ParseString(Encoding.UTF8.GetString(data));
        }
        
        public Attr ParseString(string data)
        {
            var root = new AttrDic();
            var str = data.TrimStart(new char[]{ TokenStart });
            var tokens = str.Split(new char[]{ TokenSeparator });
            for(var i = 0; i <tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                if(token.Length > 0)
                {
                    var parts = token.Split(new char[]{ TokenAssign });
                    if(parts.Length < 2)
                    {
                        var rootDic = root.AsDic;
                        if(!rootDic.ContainsKey(string.Empty))
                        {
                            rootDic.Set(string.Empty, new AttrList());
                        }
                        rootDic.Get(string.Empty).AsList.AddValue(Uri.UnescapeDataString(parts[0]));
                    }
                    else
                    {
                        var name = SplitName(Uri.UnescapeDataString(parts[0]));
                        SetAttr(root, name, new AttrString(Uri.UnescapeDataString(parts[1])));
                    }
                }
            }
            return root;
        }

    }
}