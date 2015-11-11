using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialPoint.Attributes
{
    public interface IChildParser<T> : IParser<T>
    {
        string Name { get; }

        void Load(FamilyParser<T> parent);
    }
    
    public class FamilyParser<T> : IParser<T>
    {
        const string AttrKeyType = "type";
        const string AttrKeyValue = "value";
        
        List<IChildParser<T>> _children;
        
        public FamilyParser(List<IChildParser<T>> children)
        {
            _children = children;
            foreach(var child in _children)
            {
                child.Load(this);
            }
        }

        IChildParser<T> GetChild(string type)
        {
            return _children.FirstOrDefault((p) => p.Name == type);
        }
        
        #region IParser implementation
        
        public T Parse(Attr data)
        {
            var datadic = data.AsDic;
            var type = datadic[AttrKeyType].AsValue.ToString();
            var child = GetChild(type);
            if(child == null)
            {
                throw new InvalidOperationException("No parser for type '"+type+"' found.");
            }
            return child.Parse(datadic[AttrKeyValue]);
        }
        
        #endregion
        
    }
}
