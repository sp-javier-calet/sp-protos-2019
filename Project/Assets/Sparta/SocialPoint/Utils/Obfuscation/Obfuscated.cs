namespace SocialPoint.Utils.Obfuscation
{
    public abstract class Obfuscated<T>
    {
        private static ulong _mask;
        private T _obfuscatedValue;

        public T Value
        {
            get
            {
                return Obfuscate(_obfuscatedValue);
            }
        }

        public static implicit operator T(Obfuscated<T> obfustated)
        {
            return obfustated.Value;
        }
        
        public Obfuscated(T value = default(T))
        {
            _obfuscatedValue = default(T);

            Init();
            Set(value);
        }

        protected abstract T Obfuscate(T value);

        protected void Set(T value)
        {
            _obfuscatedValue = Obfuscate(value);
        }

        private void Init()
        {
            if (0 == _mask)
            {
                uint highMask = RandomUtils.GenerateUint();
                uint lowMask = RandomUtils.GenerateUint();
                _mask = (ulong)(highMask << 32 | lowMask);
            }
        }

        protected void DoObfuscate(ref ulong value)
        {
            value ^= _mask;
        }
    }
}
