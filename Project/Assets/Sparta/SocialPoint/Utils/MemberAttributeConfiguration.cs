using System;
using System.Collections.Generic;
using System.Reflection;

namespace SocialPoint.Utils
{
    public interface IMemberAttributeObserver<A> : IDisposable, ICloneable where A : Attribute
    {
        bool Supports(object obj, Type type, A attr);

        object Apply(object obj, Type type, A attr);
    }

    public abstract class BaseMemberAttributeObserver<T,A> : IMemberAttributeObserver<A> where T : class where A : Attribute
    {
        public bool Supports(object obj, Type type, A attr)
        {
            return typeof(T) == type;
        }
        
        public object Apply(object obj, Type type, A attr)
        {
            if(typeof(T) != type)
            {
                throw new InvalidOperationException(
                    string.Format("Argument needs to be of type {0}", typeof(T).FullName));
            }
            return ApplyType((T)obj, attr);
        }

        public virtual void Dispose()
        {
        }

        public abstract object Clone();

        protected abstract T ApplyType(T obj, A attr);

    }

    public class MemberAttributeConfiguration<A> : IDisposable where A : Attribute
    {
        IList<IMemberAttributeObserver<A>> _prototypes;
        IDictionary<A,IMemberAttributeObserver<A>> _observers;
        
        public MemberAttributeConfiguration(List<IMemberAttributeObserver<A>> prototypes=null)
        {
            _observers = new Dictionary<A,IMemberAttributeObserver<A>>();
            if(prototypes == null)
            {
                prototypes = new List<IMemberAttributeObserver<A>>();
            }
            _prototypes = prototypes;
        }
        
        public virtual void Dispose()
        {
            for(int i = 0, _prototypesCount = _prototypes.Count; i < _prototypesCount; i++)
            {
                var proto = _prototypes[i];
                proto.Dispose();
            }
            _prototypes.Clear();

            var itr = _observers.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                kvp.Value.Dispose();
            }
            itr.Dispose();
            _observers.Clear();
        }
        
        public void AddObserver(IMemberAttributeObserver<A> observer)
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
        
        object Apply(object prop, Type type, A attr)
        {
            IMemberAttributeObserver<A> observer;
            if(!_observers.TryGetValue(attr, out observer))
            {
                for(int i = 0, _prototypesCount = _prototypes.Count; i < _prototypesCount; i++)
                {
                    var proto = _prototypes[i];
                    if(proto.Supports(prop, type, attr))
                    {
                        observer = (IMemberAttributeObserver<A>)proto.Clone();
                        _observers.Add(attr, observer);
                        break;
                    }
                }
            }
            if(observer != null)
            {
                return observer.Apply(prop, type, attr);
            }
            throw new InvalidOperationException(
                string.Format("Could not find any way to manage object of type '{0}'.", type.FullName));
        }

        const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        
        public void Apply(object obj)
        {
            if(obj == null)
            {
                return;
            }
            var type = obj.GetType();
            for(int i = 0, maxLength = type.GetProperties(MemberBindingFlags).Length; i < maxLength; i++)
            {
                var prop = type.GetProperties(MemberBindingFlags)[i];
                for(int j = 0, maxLength2 = prop.GetCustomAttributes(typeof(A), true).Length; j < maxLength2; j++)
                {
                    var attrObj = prop.GetCustomAttributes(typeof(A), true)[j];
                    var attr = (A)attrObj;
                    var val = prop.GetValue(obj, null);
                    val = Apply(val, prop.PropertyType, attr);
                    prop.SetValue(obj, val, null);
                }
            }
            for(int i = 0, maxLength = type.GetFields(MemberBindingFlags).Length; i < maxLength; i++)
            {
                var field = type.GetFields(MemberBindingFlags)[i];
                for(int j = 0, maxLength2 = field.GetCustomAttributes(typeof(A), true).Length; j < maxLength2; j++)
                {
                    var attrObj = field.GetCustomAttributes(typeof(A), true)[j];
                    var attr = (A)attrObj;
                    var val = field.GetValue(obj);
                    val = Apply(val, field.FieldType, attr);
                    field.SetValue(obj, val);
                }
            }
        }
    }
}