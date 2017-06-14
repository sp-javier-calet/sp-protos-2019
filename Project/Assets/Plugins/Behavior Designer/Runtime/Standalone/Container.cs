using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    public class Container
    {
        private Dictionary<string, object> _objects = new Dictionary<string, object>();
        public Dictionary<string, object> Objects
        {
            get { return _objects; }
        }

        public T Get<T>(string key)
        {
            if(!_objects.ContainsKey(key))
            {
                throw new System.MissingMemberException(string.Format("[{0}] Key {1} not found", GetType(), key));
            }
            T value = (T)_objects[key];
            return value;
        }

        public object Get(System.Type iType)
        {
            var items = _objects.GetEnumerator();
            while(items.MoveNext())
            {
                if(items.Current.Value.GetType().IsSubclassOf (iType) || items.Current.Value.GetType() == iType)
                {
                    return items.Current.Value;
                }
                System.Type [] types = items.Current.Value.GetType().GetInterfaces();
                for(int i = 0; types != null && i < types.Length; ++i)
                {
                    if(iType == types[i])
                    {
                        return items.Current.Value;
                    }
                }
            }
            return null;
        }

        public T Get<T>()
        {
            var items = _objects.GetEnumerator();
            while(items.MoveNext())
            {
                if(items.Current.Value is T)
                {
                    return (T)items.Current.Value;
                }
            }
            return default(T);
        }

        public void Set<T>(string key, T value)
        {
            _objects[key] = value;
        }

        public void Unset(string key)
        {
            _objects.Remove(key);
        }
    }
}