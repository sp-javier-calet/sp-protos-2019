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
        
        static Obfuscated()
        {
            ulong highMask = (ulong)RandomUtils.GenerateUint();
            ulong lowMask = (ulong)RandomUtils.GenerateUint();
            _mask = highMask << 32 | lowMask;
        }

        public Obfuscated(T value = default(T))
        {
            _obfuscatedValue = default(T);

            Set(value);
        }

        protected abstract T Obfuscate(T value);

        protected void Set(T value)
        {
            _obfuscatedValue = Obfuscate(value);
        }

        protected void DoObfuscate(ref ulong value)
        {
            value ^= _mask;
        }
    }
}
