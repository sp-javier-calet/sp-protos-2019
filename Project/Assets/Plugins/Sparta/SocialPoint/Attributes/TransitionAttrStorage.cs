using System;

namespace SocialPoint.Attributes
{
    public sealed class TransitionAttrStorage : IAttrStorage
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
            var attr = From.Load(key);
            if(!Attr.IsNullOrEmpty(attr))
            {
                To.Save(key, attr);
            }
            else
            {
                attr = To.Load(key);
                if(!Attr.IsNullOrEmpty(attr))
                {
                    From.Save(key, attr);
                }
            }
            return attr;
        }

        public bool Has(string key)
        {
            return From.Has(key) || To.Has(key);
        }

        public void Save(string key, Attr attr)
        {
            var old = From.Load(key);
            From.Save(key, attr);
            try
            {
                To.Save(key, attr);
            }
            catch(Exception e)
            {
                if(Attr.IsNullOrEmpty(old))
                {
                    From.Remove(key);
                }
                else
                {
                    From.Save(key, old);
                }
                throw e;
            }
        }

        public void Remove(string key)
        {
            var old = From.Load(key);
            From.Remove(key);
            try
            {
                To.Remove(key);
            }
            catch(Exception e)
            {
                if(!Attr.IsNullOrEmpty(old))
                {
                    From.Save(key, old);
                }
                throw e;
            }
        }
    }
}
