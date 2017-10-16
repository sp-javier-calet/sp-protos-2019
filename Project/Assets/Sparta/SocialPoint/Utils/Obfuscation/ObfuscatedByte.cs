using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedByte : Obfuscated<byte>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskByte
        {
            [FieldOffset(0)]
            public byte value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedByte(byte value)
        {
            return new ObfuscatedByte(value);
        }

        public ObfuscatedByte(byte value = default(byte))
            : base(value)
        {
        }

        protected override ulong Obfuscate(byte value)
        {
            UnionMaskByte reinterpret = default(UnionMaskByte);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override byte Unobfuscate(ulong value)
        {
            UnionMaskByte reinterpret = default(UnionMaskByte);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedByte operator +(ObfuscatedByte obfuscated1, ObfuscatedByte obfuscated2)
        {
            return new ObfuscatedByte((byte)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedByte operator +(ObfuscatedByte obfuscated, byte value)
        {
            return new ObfuscatedByte((byte)(obfuscated.Value + value));
        }

        public static ObfuscatedByte operator +(byte value, ObfuscatedByte obfuscated)
        {
            return new ObfuscatedByte((byte)(value + obfuscated.Value));
        }

        public static ObfuscatedByte operator -(ObfuscatedByte obfuscated1, ObfuscatedByte obfuscated2)
        {
            return new ObfuscatedByte((byte)(obfuscated1.Value - obfuscated2.Value));
        }

        public static ObfuscatedByte operator -(ObfuscatedByte obfuscated, byte value)
        {
            return new ObfuscatedByte((byte)(obfuscated.Value - value));
        }

        public static ObfuscatedByte operator -(byte value, ObfuscatedByte obfuscated)
        {
            return new ObfuscatedByte((byte)(value - obfuscated.Value));
        }

        public static ObfuscatedByte operator *(ObfuscatedByte obfuscated1, ObfuscatedByte obfuscated2)
        {
            return new ObfuscatedByte((byte)(obfuscated1.Value * obfuscated2.Value));
        }

        public static ObfuscatedByte operator *(ObfuscatedByte obfuscated, byte value)
        {
            return new ObfuscatedByte((byte)(obfuscated.Value * value));
        }

        public static ObfuscatedByte operator *(byte value, ObfuscatedByte obfuscated)
        {
            return new ObfuscatedByte((byte)(value * obfuscated.Value));
        }

        public static ObfuscatedByte operator /(ObfuscatedByte obfuscated1, ObfuscatedByte obfuscated2)
        {
            return new ObfuscatedByte((byte)(obfuscated1.Value / obfuscated2.Value));
        }

        public static ObfuscatedByte operator /(ObfuscatedByte obfuscated, byte value)
        {
            return new ObfuscatedByte((byte)(obfuscated.Value / value));
        }

        public static ObfuscatedByte operator /(byte value, ObfuscatedByte obfuscated)
        {
            return new ObfuscatedByte((byte)(value / obfuscated.Value));
        }

        public static ObfuscatedByte operator ++(ObfuscatedByte obfuscated)
        {
            obfuscated.Set((byte)(obfuscated.Value + 1));
            return obfuscated;
        }

        public static ObfuscatedByte operator --(ObfuscatedByte obfuscated)
        {
            obfuscated.Set((byte)(obfuscated.Value - 1));
            return obfuscated;
        }
    }
}
