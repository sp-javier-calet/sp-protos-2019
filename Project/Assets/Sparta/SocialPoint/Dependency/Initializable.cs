using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public interface IInitializable
    {
        void Initialize();
    }

    public class InitializableManager
    {
        Dictionary<IInitializable,int> _initialized = new Dictionary<IInitializable,int>();
        DependencyContainer _container;

        public InitializableManager(DependencyContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            var inits = _container.ResolveList<IInitializable>();
            var elements = new Dictionary<IInitializable,int>();
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
