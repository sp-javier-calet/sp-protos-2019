using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedChar : Obfuscated<char>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskChar
        {
            [FieldOffset(0)]
            public char value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedChar(char value)
        {
            return new ObfuscatedChar(value);
        }

        public ObfuscatedChar(char value = default(char))
            : base(value)
        {
        }

        protected override ulong Obfuscate(char value)
        {
            UnionMaskChar reinterpret = default(UnionMaskChar);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override char Unobfuscate(ulong value)
        {
            UnionMaskChar reinterpret = default(UnionMaskChar);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedChar operator +(ObfuscatedChar obfuscated1, ObfuscatedChar obfuscated2)
        {
            return new ObfuscatedChar((char)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedChar operator +(ObfuscatedChar obfuscated, char value)
        {
            return new ObfuscatedChar((char)(obfuscated.Value + value));
        }

        public static ObfuscatedChar operator +(char value, ObfuscatedChar obfuscated)
        {
            return new ObfuscatedChar((char)(value + obfuscated.Value));
        }

        public static ObfuscatedChar operator -(ObfuscatedChar obfuscated1, ObfuscatedChar obfuscated2)
        {
            return new ObfuscatedChar((char)(obfuscated1.Value - obfuscated2.Value));
        }

        public static ObfuscatedChar operator -(ObfuscatedChar obfuscated, char value)
        {
            return new ObfuscatedChar((char)(obfuscated.Value - value));
        }

        public static ObfuscatedChar operator -(char value, ObfuscatedChar obfuscated)
        {
            return new ObfuscatedChar((char)(value - obfuscated.Value));
        }

        public static ObfuscatedChar operator *(ObfuscatedChar obfuscated1, ObfuscatedChar obfuscated2)
        {
            return new ObfuscatedChar((char)(obfuscated1.Value * obfuscated2.Value));
        }

        public static ObfuscatedChar operator *(ObfuscatedChar obfuscated, char value)
        {
            return new ObfuscatedChar((char)(obfuscated.Value * value));
        }

        public static ObfuscatedChar operator *(char value, ObfuscatedChar obfuscated)
        {
            return new ObfuscatedChar((char)(value * obfuscated.Value));
        }

        public static ObfuscatedChar operator /(ObfuscatedChar obfuscated1, ObfuscatedChar obfuscated2)
        {
            return new ObfuscatedChar((char)(obfuscated1.Value / obfuscated2.Value));
        }

        public static ObfuscatedChar operator /(ObfuscatedChar obfuscated, char value)
        {
            return new ObfuscatedChar((char)(obfuscated.Value / value));
        }

        public static ObfuscatedChar operator /(char value, ObfuscatedChar obfuscated)
        {
            return new ObfuscatedChar((char)(value / obfuscated.Value));
        }

        public static ObfuscatedChar operator ++(ObfuscatedChar obfuscated)
        {
            obfuscated.Set((char)(obfuscated.Value + 1));
            return obfuscated;
        }

        public static ObfuscatedChar operator --(ObfuscatedChar obfuscated)
        {
            obfuscated.Set((char)(obfuscated.Value - 1));
            return obfuscated;
        }
    }
}
