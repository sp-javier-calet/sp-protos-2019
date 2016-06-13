//  Author:
//    Miguel-Janer 

using System;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public class AttrPatcher
    {
        const char PointerSeparator = '/';
        const char EscapedPointerSeparator = '~';
        const string EscapedEscapedPointerSeparator = "~0";
        const string TokenHelper = ":token:";
            
        const string OpKey = "op";
        const string PathKey = "path";
        const string ValueKey = "value";
        const string AddKey = "add";

        public bool Patch(AttrList patch, Attr data)
        {
            for(int i = 0; i < patch.Count; i++)
            {
                var op = patch.Get(i).AsDic;
                var name = op.GetValue(OpKey).ToString();
                var path = op.GetValue(PathKey).ToString();
                switch(name)
                {
                case AddKey:
                    if(!Add(path, op.GetValue(ValueKey), data))
                    {
                        return false;
                    }
                    break;

                default:
                    return false;
                }

            }
            return true;
        }

        bool Add(string path, Attr value, Attr data)
        {
            var parts = SplitPath(path);
            var last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            var parent = AttrPatcherGet(parts, data);
            if(parent.IsDic)
            {
                parent.AsDic.Set(last, value);
            }
            else
            if(parent.IsList)
            {
                parent.AsList.Add(value);
            }
            else
            {
                return false;
            }
            return true;
        }

        Attr AttrPatcherGet(List<string> parts, Attr data)
        {
            Attr elm = data;
            for(int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if(data.IsList)
                {
                    elm = data.AsList.Get(int.Parse(part));
                }
                else
                if(data.IsDic)
                {
                    elm = data.AsDic.Get(part);
                }
                else
                {
                    return new AttrEmpty();
                }
            }
            return elm;
        }

        public List<string> SplitPath(string path)
        {
            var fpath = path.TrimStart(PointerSeparator);
            var parts = new List<string>(fpath.Split(PointerSeparator));
            for(int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                part = part.Replace(EscapedEscapedPointerSeparator, TokenHelper);
                part = part.Replace(EscapedPointerSeparator, PointerSeparator);
                parts[i] = part.Replace(TokenHelper, EscapedPointerSeparator.ToString());
            }
            return parts;
        }
    }
}

