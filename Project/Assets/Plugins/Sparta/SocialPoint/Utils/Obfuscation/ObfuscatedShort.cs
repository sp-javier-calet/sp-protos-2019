using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedShort : Obfuscated<short>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskShort
        {
            [FieldOffset(0)]
            public short value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedShort(short value)
        {
            return new ObfuscatedShort(value);
        }

        public ObfuscatedShort(short value = default(short))
            : base(value)
        {
        }

        protected override ulong Obfuscate(short value)
        {
            UnionMaskShort reinterpret = default(UnionMaskShort);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override short Unobfuscate(ulong value)
        {
            UnionMaskShort reinterpret = default(UnionMaskShort);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedShort operator +(ObfuscatedShort obfuscated1, ObfuscatedShort obfuscated2)
        {
            return new ObfuscatedShort((short)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedShort operator +(ObfuscatedShort obfuscated, short value)
        {
            return new ObfuscatedShort((short)(obfuscated.Value + value));
        }

        public static ObfuscatedShort operator +(short value, ObfuscatedShort obfuscated)
        {
            return new ObfuscatedShort((short)(value + obfuscated.Value));
        }

        public static ObfuscatedShort operator -(ObfuscatedShort obfuscated1, ObfuscatedShort obfuscated2)
        {
            return new ObfuscatedShort((short)(obfuscated1.Value - obfuscated2.Value));
        }

        public static ObfuscatedShort operator -(ObfuscatedShort obfuscated, short value)
        {
            return new ObfuscatedShort((short)(obfuscated.Value - value));
        }

        public static ObfuscatedShort operator -(short value, ObfuscatedShort obfuscated)
        {
            return new ObfuscatedShort((short)(value - obfuscated.Value));
        }

        public static ObfuscatedShort operator *(ObfuscatedShort obfuscated1, ObfuscatedShort obfuscated2)
        {
            return new ObfuscatedShort((short)(obfuscated1.Value * obfuscated2.Value));
        }

        public static ObfuscatedShort operator *(ObfuscatedShort obfuscated, short value)
        {
            return new ObfuscatedShort((short)(obfuscated.Value * value));
        }

        public static ObfuscatedShort operator *(short value, ObfuscatedShort obfuscated)
        {
            return new ObfuscatedShort((short)(value * obfuscated.Value));
        }

        public static ObfuscatedShort operator /(ObfuscatedShort obfuscated1, ObfuscatedShort obfuscated2)
        {
            return new ObfuscatedShort((short)(obfuscated1.Value / obfuscated2.Value));
        }

        public static ObfuscatedShort operator /(ObfuscatedShort obfuscated, short value)
        {
            return new ObfuscatedShort((short)(obfuscated.Value / value));
        }

        public static ObfuscatedShort operator /(short value, ObfuscatedShort obfuscated)
        {
            return new ObfuscatedShort((short)(value / obfuscated.Value));
        }

        public static ObfuscatedShort operator ++(ObfuscatedShort obfuscated)
        {
            obfuscated.Set((short)(obfuscated.Value + 1));
            return obfuscated;
        }

        public static ObfuscatedShort operator --(ObfuscatedShort obfuscated)
        {
            obfuscated.Set((short)(obfuscated.Value - 1));
            return obfuscated;
        }
    }
}
