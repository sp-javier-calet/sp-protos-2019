using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public interface IInitializable
    {
        void Initialize();
    }

    public class InitializableComparer : IEqualityComparer<IInitializable>
    {
        public int GetHashCode(IInitializable obj)
        {
            return obj.GetType().GetHashCode();
        }

        public bool Equals(IInitializable x, IInitializable y)
        {
            return ReferenceEquals(x, y);
        }
    }

    public class InitializableManager
    {
        Dictionary<IInitializable,int> _initialized;
        DependencyContainer _container;
        InitializableComparer _comparer;

        public InitializableManager(DependencyContainer container)
        {
            _container = container;
            _comparer = new InitializableComparer();
            _initialized = new Dictionary<IInitializable, int>(_comparer);
        }

        public void Initialize()
        {
            var inits = _container.ResolveList<IInitializable>();
            var elements = new Dictionary<IInitializable,int>(_comparer);
            for(var i = 0; i < inits.Count; i++)
            {
                var init = inits[i];
                if(!elements.ContainsKey(init))
                {
                    elements[init] = 1;
                }
                else
                {
                    elements[init]++;
                }
            }
            var itr = elements.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                var times = 0;
                _initialized.TryGetValue(elm.Key, out times);
                var diff = elm.Value - times;
                for(var i = 0; i < diff; i++)
                {
                    elm.Key.Initialize();
                }
                _initialized[elm.Key] = elm.Value;
            }
            itr.Dispose();
        }
    }
}
