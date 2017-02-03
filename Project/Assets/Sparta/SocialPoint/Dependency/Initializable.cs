using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public sealed class InitializableManager
    {
        readonly Dictionary<IInitializable,int> _initialized;
        readonly DependencyContainer _container;
        readonly ReferenceComparer<IInitializable> _comparer;

        public InitializableManager(DependencyContainer container)
        {
            _container = container;
            _comparer = new ReferenceComparer<IInitializable>();
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
