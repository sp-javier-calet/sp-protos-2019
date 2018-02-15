using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedUInt : Obfuscated<uint>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskUInt
        {
            [FieldOffset(0)]
            public uint value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedUInt(uint value)
        {
            return new ObfuscatedUInt(value);
        }

        public ObfuscatedUInt(uint value = default(uint))
            : base(value)
        {
        }

        protected override ulong Obfuscate(uint value)
        {
            UnionMaskUInt reinterpret = default(UnionMaskUInt);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override uint Unobfuscate(ulong value)
        {
            UnionMaskUInt reinterpret = default(UnionMaskUInt);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedUInt operator +(ObfuscatedUInt obfuscated1, ObfuscatedUInt obfuscated2)
        {
            return new ObfuscatedUInt(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedUInt operator +(ObfuscatedUInt obfuscated, uint value)
        {
            return new ObfuscatedUInt(obfuscated.Value + value);
        }

        public static ObfuscatedUInt operator +(uint value, ObfuscatedUInt obfuscated)
        {
            return new ObfuscatedUInt(value + obfuscated.Value);
        }

        public static ObfuscatedUInt operator -(ObfuscatedUInt obfuscated1, ObfuscatedUInt obfuscated2)
        {
            return new ObfuscatedUInt(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedUInt operator -(ObfuscatedUInt obfuscated, uint value)
        {
            return new ObfuscatedUInt(obfuscated.Value - value);
        }

        public static ObfuscatedUInt operator -(uint value, ObfuscatedUInt obfuscated)
        {
            return new ObfuscatedUInt(value - obfuscated.Value);
        }

        public static ObfuscatedUInt operator *(ObfuscatedUInt obfuscated1, ObfuscatedUInt obfuscated2)
        {
            return new ObfuscatedUInt(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedUInt operator *(ObfuscatedUInt obfuscated, uint value)
        {
            return new ObfuscatedUInt(obfuscated.Value * value);
        }

        public static ObfuscatedUInt operator *(uint value, ObfuscatedUInt obfuscated)
        {
            return new ObfuscatedUInt(value * obfuscated.Value);
        }

        public static ObfuscatedUInt operator /(ObfuscatedUInt obfuscated1, ObfuscatedUInt obfuscated2)
        {
            return new ObfuscatedUInt(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedUInt operator /(ObfuscatedUInt obfuscated, uint value)
        {
            return new ObfuscatedUInt(obfuscated.Value / value);
        }

        public static ObfuscatedUInt operator /(uint value, ObfuscatedUInt obfuscated)
        {
            return new ObfuscatedUInt(value / obfuscated.Value);
        }

        public static ObfuscatedUInt operator ++(ObfuscatedUInt obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1);
            return obfuscated;
        }

        public static ObfuscatedUInt operator --(ObfuscatedUInt obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1);
            return obfuscated;
        }
    }
}
