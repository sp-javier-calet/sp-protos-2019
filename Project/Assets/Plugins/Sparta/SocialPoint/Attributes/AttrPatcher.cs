//  Author:
//      Miguel-Janer 
//
//      port from: https://github.com/socialpoint/sp-hydra-engine/blob/develop/src/hydra/attr/AttrPatcher.hpp
//      
//  Implementation of json pointer and patch standards
//      http://tools.ietf.org/html/rfc6901
//      http://tools.ietf.org/html/rfc6902

using System;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public sealed class AttrPatcher
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

        AttrDic CreatePatchOperation(string key, string path, Attr value = null)
        {
            AttrDic op = new AttrDic();
            op.SetValue(OpKey, key);
            op.SetValue(PathKey, path);
            if(value != null)
            {
                op.Set(ValueKey, value);
            }
            return op;
        }

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

        public AttrList Diff(Attr origin, Attr to)
        {
            AttrList patch = new AttrList();
            Diff(origin, to, patch, "");
            return patch;
        }

        public void Diff(Attr origin, Attr to, AttrList patch, string path)
        {
            if(origin.AttrType != to.AttrType || origin.AttrType == AttrType.VALUE)
            {
                if(origin != to)
                {
                    AttrDic op = CreatePatchOperation(ReplaceKey, path, (Attr)to.Clone());
                    patch.Add(op);
                }
                return;
            }
            else if(origin.AttrType == AttrType.LIST)
            {
                var originList = origin.AsList;
                var toList = to.AsList;
                for(int i = 0; i < originList.Count && i < toList.Count; i++)
                {
                    Diff(originList.Get(i), toList.Get(i), patch, AddPath(path, i)); 
                }
                if(originList.Count > toList.Count)
                {
                    for(int i = originList.Count - 1; i >= toList.Count; i--)
                    {
                        AttrDic op = CreatePatchOperation(RemoveKey, AddPath(path, i));
                        patch.Add(op);
                    }
                }
                else if(originList.Count < toList.Count)
                {
                    for(int i = originList.Count; i < toList.Count; i++)
                    {
                        AttrDic op = CreatePatchOperation(AddKey, AddPath(path, i), (Attr)toList.Get(i).Clone());
                        patch.Add(op);
                    }
                }
            }
            else if(origin.AttrType == AttrType.DICTIONARY)
            {
                var originDict = origin.AsDic;
                var toDict = to.AsDic;
                var itr = originDict.GetEnumerator();
                while(itr.MoveNext())
                {
                    var key = itr.Current.Key;
                    var kpath = AddPath(path, key);
                    if(toDict.ContainsKey(key))
                    {
                        Diff(itr.Current.Value, toDict.Get(key), patch, kpath);
                    }
                    else
                    {
                        AttrDic op = CreatePatchOperation(RemoveKey, kpath, (Attr)toDict.Get(key).Clone());
                        patch.Add(op);
                    }
                }
                itr.Dispose();
                itr = toDict.GetEnumerator();
                while(itr.MoveNext())
                {
                    var key = itr.Current.Key;
                    if(!originDict.ContainsKey(key))
                    {
                        AttrDic op = CreatePatchOperation(AddKey, AddPath(path, key), (Attr)itr.Current.Value.Clone());
                        patch.Add(op);
                    }
                }
                itr.Dispose();
            }
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

        string AddPath<T>(string path, T key)
        {
            string fkey = key.ToString();
            fkey.Replace(EscapedPointerSeparator.ToString(), EscapedEscapedPointerSeparator);
            fkey.Replace(PointerSeparator, EscapedPointerSeparator);
            return string.Concat(path, PointerSeparator, fkey);
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

