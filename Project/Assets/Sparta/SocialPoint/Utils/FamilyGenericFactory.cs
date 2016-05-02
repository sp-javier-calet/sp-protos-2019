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
                foreach(var factory in factories)
                {
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
            foreach(var factory in _factories)
            {
                if(factory.SupportsModel(model))
                {
                    return factory;
                }
            }
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