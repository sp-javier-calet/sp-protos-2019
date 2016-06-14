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
        const string AddKey = "add";
        const string RemoveKey = "remove";
        const string ReplaceKey = "replace";
        const string MoveKey = "move";
        const string CopyKey = "copy";
        const string TestKey = "test";
        const string PathKey = "path";
        const string ValueKey = "value";
        const string FromKey = "from";

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
                case RemoveKey:
                    if(!Remove(path, data))
                    {
                        return false;
                    }
                    break;
                case ReplaceKey:
                    if(!Replace(path, op.GetValue(ValueKey), data))
                    {
                        return  false;
                    }
                    break;
                case MoveKey:
                    if(!Move(op.GetValue(FromKey).ToString(), path, data))
                    {
                        return false;
                    }
                    break;
                case CopyKey:
                    if(!Copy(op.GetValue(FromKey).ToString(), path, data))
                    {
                        return false;
                    }
                    break;
                case TestKey:
                    if(!Test(path, op.GetValue(ValueKey), data))
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
            else if(parent.IsList)
            {
                parent.AsList.Insert(int.Parse(last), value);
            }
            else
            {
                return false;
            }
            return true;
        }

        bool Remove(string path, Attr data)
        {
            var parts = SplitPath(path);
            var last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            var parent = AttrPatcherGet(parts, data);
            if(parent.IsDic)
            {
                parent.AsDic.Remove(last);
            }
            else if(parent.IsList)
            {
                parent.AsList.RemoveAt(int.Parse(last));
            }
            else
            {
                return false;
            }
            return true;
        }

        bool Replace(string path, Attr value, Attr data)
        {
            var parts = SplitPath(path);
            var last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            var parent = AttrPatcherGet(parts, data);
            if(parent.IsDic)
            {
                parent.AsDic.Set(last, value);
            }
            else if(parent.IsList)
            {
                parent.AsList.Set(int.Parse(last), value);
            }
            else
            {
                return false;
            }
            return true;
        }

        bool Move(string origin, string path, Attr data)
        {
            var parts = SplitPath(origin);
            var last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            var parent = AttrPatcherGet(parts, data);
            if(parent.IsDic)
            {
                var value = parent.AsDic.Get(last);
                parent.AsDic.Remove(last);
                if(!Add(path, value, data))
                {
                    parent.AsDic.Set(last, value);//revert
                    return false;
                }
                return true;
            }
            else if(parent.IsList)
            {
                var value = parent.AsList.Get(int.Parse(last));
                parent.AsList.RemoveAt(int.Parse(last));
                if(!Add(path, value, data))
                {
                    parent.AsList.Set(int.Parse(last), value);//revert
                    return false;
                }
                return true;
            }

            return false;
        }

        bool Copy(string origin, string path, Attr data)
        {
            var parts = SplitPath(origin);
            var last = parts[parts.Count - 1];
            parts.RemoveAt(parts.Count - 1);
            var parent = AttrPatcherGet(parts, data);
            if(parent.IsDic)
            {
                var value = parent.AsDic.Get(last);
                return Add(path, (Attr)value.Clone(), data);
            }
            else if(parent.IsList)
            {
                var value = parent.AsList.Get(int.Parse(last));
                return Add(path, (Attr)value.Clone(), data);
            }
            return false;
        }

        bool Test(string path, Attr value, Attr data)
        {
            return Get(path, data) == value;
        }

        Attr AttrPatcherGet(List<string> parts, Attr data)
        {
            Attr elm = data;
            for(int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if(elm.IsList)
                {
                    elm = elm.AsList.Get(int.Parse(part));
                }
                else if(data.IsDic)
                {
                    elm = elm.AsDic.Get(part);
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

        Attr Get(string path, Attr data)
        {
            var parts = SplitPath(path);
            return AttrPatcherGet(parts, data);
        }
    }
}

