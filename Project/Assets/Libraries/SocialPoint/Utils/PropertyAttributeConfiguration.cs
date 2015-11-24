using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public interface IPropertyAttributeObserver<T> : IDisposable, ICloneable where T : Attribute
    {
        bool Supports(object obj, T attr);

        object Apply(object obj, T attr);
    }

    public abstract class BasePropertyAttributeObserver<T,A> : IPropertyAttributeObserver<A> where T : class where A : Attribute
    {
        public bool Supports(object obj, A attr)
        {
            return obj as T != null;
        }
        
        public object Apply(object obj, A attr)
        {
            var tobj = obj as T;
            if(tobj == null)
            {
                throw new InvalidOperationException(
                    string.Format("Argument needs to be of type {0}", typeof(T).FullName));
            }
            return ApplyType(tobj, attr);
        }

        public virtual void Dispose()
        {
        }

        public abstract object Clone();

        protected abstract T ApplyType(T obj, A attr);

    }

    public class PropertyAttributeConfiguration<T> : IDisposable where T : Attribute
    {
        IList<IPropertyAttributeObserver<T>> _prototypes;
        IDictionary<T,IPropertyAttributeObserver<T>> _observers;
        
        public PropertyAttributeConfiguration(List<IPropertyAttributeObserver<T>> prototypes=null)
        {
            _observers = new Dictionary<T,IPropertyAttributeObserver<T>>();
            if(prototypes == null)
            {
                prototypes = new List<IPropertyAttributeObserver<T>>();
            }
            _prototypes = prototypes;
        }
        
        public virtual void Dispose()
        {
            foreach(var proto in _prototypes)
            {
                proto.Dispose();
            }
            _prototypes.Clear();
            foreach(var kvp in _observers)
            {
                kvp.Value.Dispose();
            }
            _observers.Clear();
        }
        
        public void AddObserver(IPropertyAttributeObserver<T> observer)
        {
            if(observer == null)
            {
                throw new ArgumentNullException("observer");
            }
            if(!_prototypes.Contains(observer))
            {
                _prototypes.Add(observer);
            }
        }
        
        object Apply(object prop, T attr)
        {
            IPropertyAttributeObserver<T> observer = null;
            if(!_observers.TryGetValue(attr, out observer))
            {
                foreach(var proto in _prototypes)
                {
                    if(proto.Supports(prop, attr))
                    {
                        observer = (IPropertyAttributeObserver<T>)proto.Clone();
                        _observers.Add(attr, observer);
                        break;
                    }
                }
            }
            if(observer != null)
            {
                return observer.Apply(prop, attr);
            }
            throw new InvalidOperationException(
                string.Format("Could not find any way to manage object of type '{0}'.", prop.GetType().FullName));
        }
        
        public void Apply(object obj)
        {
            foreach(var prop in obj.GetType().GetProperties())
            {
                foreach(var attr in prop.GetCustomAttributes(false))
                {
                    var tattr = attr as T;
                    if(tattr != null)
                    {
                        var val = prop.GetValue(obj, null);
                        val = Apply(val, tattr);
                        prop.SetValue(obj, val, null);
                        break;
                    }
                }
            }
        }
    }
}