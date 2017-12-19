namespace SocialPoint.Utils.Obfuscation
{
    public abstract class Obfuscated
    {
        protected static ulong _mask;

        static Obfuscated()
        {
            // Mask must never be 0.
            ulong highMask = (ulong)RandomUtils.Range(1, uint.MaxValue);
            ulong lowMask = (ulong)RandomUtils.Range(1, uint.MaxValue);
            _mask = highMask << 32 | lowMask;
        }
    }

    public abstract class Obfuscated<T> : Obfuscated
    {
        
        private ulong _obfuscatedValue;

        public ulong ObfuscatedValue
        {
            get
            {
                return _obfuscatedValue;
            }
        }

        public T Value
        {
            get
            {
                return Unobfuscate(_obfuscatedValue);
            }
        }

        public static implicit operator T(Obfuscated<T> obfustated)
        {
            return obfustated.Value;
        }

        public Obfuscated(T value = default(T))
        {
            Set(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        protected abstract ulong Obfuscate(T value);
        protected abstract T Unobfuscate(ulong value);

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
