using System;

namespace SocialPoint.Attributes
{
    class TransitionAttrStorage : IAttrStorage
    {
        public IAttrStorage From;
        public IAttrStorage To;


        public TransitionAttrStorage(IAttrStorage from, IAttrStorage to)
        {
            From = from;
            To = to;
        }

        public Attr Load(string key)
        {
            Attr attr = null;
            if(From.Has(key))
            {
                attr = From.Load(key);
                To.Save(key, attr);
            }
            if(attr == null && To.Has(key))
            {
                attr = To.Load(key);
                From.Save(key, attr);
            }
            return attr;
        }

        public bool Has(string key)
        {
            return From.Has(key) || To.Has(key);
        }

        public void Save(string key, Attr attr)
        {
            From.Save(key, attr);
            To.Save(key, attr);
        }

        public void Remove(string key)
        {
            From.Remove(key);
            To.Remove(key);
        }
    }
}
