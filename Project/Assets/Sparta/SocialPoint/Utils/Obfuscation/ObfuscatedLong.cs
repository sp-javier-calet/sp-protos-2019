using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedLong : Obfuscated<long>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskLong
        {
            [FieldOffset(0)]
            public long value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedLong(long value)
        {
            return new ObfuscatedLong(value);
        }

        public ObfuscatedLong(long value = default(long))
            : base(value)
        {
        }

        protected override ulong Obfuscate(long value)
        {
            UnionMaskLong reinterpret = default(UnionMaskLong);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override long Unobfuscate(ulong value)
        {
            UnionMaskLong reinterpret = default(UnionMaskLong);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedLong operator +(ObfuscatedLong obfuscated1, ObfuscatedLong obfuscated2)
        {
            return new ObfuscatedLong(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedLong operator +(ObfuscatedLong obfuscated, long value)
        {
            return new ObfuscatedLong(obfuscated.Value + value);
        }

        public static ObfuscatedLong operator +(long value, ObfuscatedLong obfuscated)
        {
            return new ObfuscatedLong(value + obfuscated.Value);
        }

        public static ObfuscatedLong operator -(ObfuscatedLong obfuscated1, ObfuscatedLong obfuscated2)
        {
            return new ObfuscatedLong(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedLong operator -(ObfuscatedLong obfuscated, long value)
        {
            return new ObfuscatedLong(obfuscated.Value - value);
        }

        public static ObfuscatedLong operator -(long value, ObfuscatedLong obfuscated)
        {
            return new ObfuscatedLong(value - obfuscated.Value);
        }

        public static ObfuscatedLong operator *(ObfuscatedLong obfuscated1, ObfuscatedLong obfuscated2)
        {
            return new ObfuscatedLong(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedLong operator *(ObfuscatedLong obfuscated, long value)
        {
            return new ObfuscatedLong(obfuscated.Value * value);
        }

        public static ObfuscatedLong operator *(long value, ObfuscatedLong obfuscated)
        {
            return new ObfuscatedLong(value * obfuscated.Value);
        }

        public static ObfuscatedLong operator /(ObfuscatedLong obfuscated1, ObfuscatedLong obfuscated2)
        {
            return new ObfuscatedLong(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedLong operator /(ObfuscatedLong obfuscated, long value)
        {
            return new ObfuscatedLong(obfuscated.Value / value);
        }

        public static ObfuscatedLong operator /(long value, ObfuscatedLong obfuscated)
        {
            return new ObfuscatedLong(value / obfuscated.Value);
        }

        public static ObfuscatedLong operator ++(ObfuscatedLong obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1);
            return obfuscated;
        }

        public static ObfuscatedLong operator --(ObfuscatedLong obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1);
            return obfuscated;
        }
    }
}
