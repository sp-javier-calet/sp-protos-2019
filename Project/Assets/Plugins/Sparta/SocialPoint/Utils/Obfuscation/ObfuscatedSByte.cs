using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedSByte : Obfuscated<sbyte>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskSByte
        {
            [FieldOffset(0)]
            public sbyte value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedSByte(sbyte value)
        {
            return new ObfuscatedSByte(value);
        }

        public ObfuscatedSByte(sbyte value = default(sbyte))
            : base(value)
        {
        }

        protected override ulong Obfuscate(sbyte value)
        {
            UnionMaskSByte reinterpret = default(UnionMaskSByte);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override sbyte Unobfuscate(ulong value)
        {
            UnionMaskSByte reinterpret = default(UnionMaskSByte);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedSByte operator +(ObfuscatedSByte obfuscated1, ObfuscatedSByte obfuscated2)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedSByte operator +(ObfuscatedSByte obfuscated, sbyte value)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated.Value + value));
        }

        public static ObfuscatedSByte operator +(sbyte value, ObfuscatedSByte obfuscated)
        {
            return new ObfuscatedSByte((sbyte)(value + obfuscated.Value));
        }

        public static ObfuscatedSByte operator -(ObfuscatedSByte obfuscated1, ObfuscatedSByte obfuscated2)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated1.Value - obfuscated2.Value));
        }

        public static ObfuscatedSByte operator -(ObfuscatedSByte obfuscated, sbyte value)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated.Value - value));
        }

        public static ObfuscatedSByte operator -(sbyte value, ObfuscatedSByte obfuscated)
        {
            return new ObfuscatedSByte((sbyte)(value - obfuscated.Value));
        }

        public static ObfuscatedSByte operator *(ObfuscatedSByte obfuscated1, ObfuscatedSByte obfuscated2)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated1.Value * obfuscated2.Value));
        }

        public static ObfuscatedSByte operator *(ObfuscatedSByte obfuscated, sbyte value)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated.Value * value));
        }

        public static ObfuscatedSByte operator *(sbyte value, ObfuscatedSByte obfuscated)
        {
            return new ObfuscatedSByte((sbyte)(value * obfuscated.Value));
        }

        public static ObfuscatedSByte operator /(ObfuscatedSByte obfuscated1, ObfuscatedSByte obfuscated2)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated1.Value / obfuscated2.Value));
        }

        public static ObfuscatedSByte operator /(ObfuscatedSByte obfuscated, sbyte value)
        {
            return new ObfuscatedSByte((sbyte)(obfuscated.Value / value));
        }

        public static ObfuscatedSByte operator /(sbyte value, ObfuscatedSByte obfuscated)
        {
            return new ObfuscatedSByte((sbyte)(value / obfuscated.Value));
        }

        public static ObfuscatedSByte operator ++(ObfuscatedSByte obfuscated)
        {
            obfuscated.Set((sbyte)(obfuscated.Value + 1));
            return obfuscated;
        }

        public static ObfuscatedSByte operator --(ObfuscatedSByte obfuscated)
        {
            obfuscated.Set((sbyte)(obfuscated.Value - 1));
            return obfuscated;
        }
    }
}
