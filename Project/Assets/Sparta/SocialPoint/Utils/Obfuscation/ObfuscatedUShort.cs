using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedUShort : Obfuscated<ushort>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskUShort
        {
            [FieldOffset(0)]
            public ushort value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public ObfuscatedUShort(ushort value = default(ushort))
            : base(value)
        {
        }

        protected override ushort Obfuscate(ushort value)
        {
            UnionMaskUShort reinterpret = default(UnionMaskUShort);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedUShort operator +(ObfuscatedUShort obfuscated1, ObfuscatedUShort obfuscated2)
        {
            return new ObfuscatedUShort((ushort)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedUShort operator +(ObfuscatedUShort obfuscated, ushort value)
        {
            return new ObfuscatedUShort((ushort)(obfuscated.Value + value));
        }

        public static ObfuscatedUShort operator +(ushort value, ObfuscatedUShort obfuscated)
        {
            return new ObfuscatedUShort((ushort)(value + obfuscated.Value));
        }

        public static ObfuscatedUShort operator -(ObfuscatedUShort obfuscated1, ObfuscatedUShort obfuscated2)
        {
            return new ObfuscatedUShort((ushort)(obfuscated1.Value + obfuscated2.Value));
        }

        public static ObfuscatedUShort operator -(ObfuscatedUShort obfuscated, ushort value)
        {
            return new ObfuscatedUShort((ushort)(obfuscated.Value - value));
        }

        public static ObfuscatedUShort operator -(ushort value, ObfuscatedUShort obfuscated)
        {
            return new ObfuscatedUShort((ushort)(value - obfuscated.Value));
        }

        public static ObfuscatedUShort operator *(ObfuscatedUShort obfuscated1, ObfuscatedUShort obfuscated2)
        {
            return new ObfuscatedUShort((ushort)(obfuscated1.Value * obfuscated2.Value));
        }

        public static ObfuscatedUShort operator *(ObfuscatedUShort obfuscated, ushort value)
        {
            return new ObfuscatedUShort((ushort)(obfuscated.Value * value));
        }

        public static ObfuscatedUShort operator *(ushort value, ObfuscatedUShort obfuscated)
        {
            return new ObfuscatedUShort((ushort)(value * obfuscated.Value));
        }

        public static ObfuscatedUShort operator /(ObfuscatedUShort obfuscated1, ObfuscatedUShort obfuscated2)
        {
            return new ObfuscatedUShort((ushort)(obfuscated1.Value / obfuscated2.Value));
        }

        public static ObfuscatedUShort operator /(ObfuscatedUShort obfuscated, ushort value)
        {
            return new ObfuscatedUShort((ushort)(obfuscated.Value / value));
        }

        public static ObfuscatedUShort operator /(ushort value, ObfuscatedUShort obfuscated)
        {
            return new ObfuscatedUShort((ushort)(value / obfuscated.Value));
        }

        public static ObfuscatedUShort operator ++(ObfuscatedUShort obfuscated)
        {
            obfuscated.Set((ushort)(obfuscated.Value + 1));
            return obfuscated;
        }

        public static ObfuscatedUShort operator --(ObfuscatedUShort obfuscated)
        {
            obfuscated.Set((ushort)(obfuscated.Value - 1));
            return obfuscated;
        }
    }
}
