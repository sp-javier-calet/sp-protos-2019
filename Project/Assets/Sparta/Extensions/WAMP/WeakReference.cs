using System;

namespace SocialPoint.Utils
{
    public sealed class WeakReference<T> : WeakReference
    {

        public WeakReference(object target) : base(target)
        {
        }


        public WeakReference(object target, bool trackResurrection) : base(target, trackResurrection)
        {
        }

        public new T Target
        {
            get
            {
                return (T)((WeakReference)(this)).Target;
            }
            set
            {
                Target = value;
            }
        }

    }
}