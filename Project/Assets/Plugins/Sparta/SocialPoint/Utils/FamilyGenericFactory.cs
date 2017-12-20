using System.Collections.Generic;
using System;

namespace SocialPoint.Utils
{
    public class FamilyGenericFactory<M, C> : IGenericFactory<M, C>
    {
        HashSet<IGenericFactory<M, C>> _factories = new HashSet<IGenericFactory<M, C>>();

        public FamilyGenericFactory(List<IGenericFactory<M, C>> factories = null)
        {
            if(factories != null)
            {
                for(int i = 0, factoriesCount = factories.Count; i < factoriesCount; i++)
                {
                    var factory = factories[i];
                    Add(factory);
                }
            }
        }

        public void Add(IGenericFactory<M, C> factory)
        {
            if(factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            _factories.Add(factory);
        }

        public bool SupportsModel(M model)
        {
            return GetSpecificFactory(model) != null;
        }

        IGenericFactory<M, C> GetSpecificFactory(M model)
        {
            var itr = _factories.GetEnumerator();
            while(itr.MoveNext())
            {
                var factory = itr.Current;
                if(factory.SupportsModel(model))
                {
                    itr.Dispose();
                    return factory;
                }
            }
            itr.Dispose();
            return null;
        }

        public C Create(M model)
        {
            var factory = GetSpecificFactory(model);
            if(factory != null)
            {
                return factory.Create(model);
            }
            throw new InvalidOperationException("Could not find any factory that supports this model");
        }
    }
}