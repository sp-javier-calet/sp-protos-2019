using System.Collections.Generic;
using SocialPoint.Base;
using System;

namespace SocialPoint.Attributes
{
    public interface IChildParser<T> : IAttrObjParser<T>
    {
        string Name { get; }

        FamilyParser<T> Parent { set; }
    }
        
    public sealed class FamilyParser<T> : IAttrObjParser<T>

    {
        const string AttrKeyType = "type";
        const string AttrKeyValue = "value";
        
        List<IChildParser<T>> _children;

        public FamilyParser(IChildParser<T>[] children) :
            this(new List<IChildParser<T>>(children))
        {
        }

        public FamilyParser(List<IChildParser<T>> children)
        {
            _children = children;
            for(int i = 0, _childrenCount = _children.Count; i < _childrenCount; i++)
            {
                var child = _children[i];
                child.Parent = this;
            }
        }

        IChildParser<T> GetChild(string type)
        {
            return _children.FirstOrDefault((p) => p.Name == type);
        }

        #region IParser implementation

        public T Parse(Attr data)
        {
            if(Attr.IsNullOrEmpty(data))
            {
                return default(T);
            }
            var datadic = data.AsDic;
            var type = datadic[AttrKeyType].AsValue.ToString();
            var child = GetChild(type);
            if(child == null)
            {
                throw new InvalidOperationException("No parser for type '" + type + "' found.");
            }
            return child.Parse(datadic[AttrKeyValue]);
        }

        #endregion
        
    }
}
