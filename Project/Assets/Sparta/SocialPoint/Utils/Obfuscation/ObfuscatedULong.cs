using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedULong : Obfuscated<ulong>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskULong
        {
            [FieldOffset(0)]
            public ulong value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedULong(ulong value)
        {
            return new ObfuscatedULong(value);
        }

        public ObfuscatedULong(ulong value = default(ulong))
            : base(value)
        {
        }

        protected override ulong Obfuscate(ulong value)
        {
            UnionMaskULong reinterpret = default(UnionMaskULong);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override ulong Unobfuscate(ulong value)
        {
            UnionMaskULong reinterpret = default(UnionMaskULong);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedULong operator +(ObfuscatedULong obfuscated1, ObfuscatedULong obfuscated2)
        {
            return new ObfuscatedULong(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedULong operator +(ObfuscatedULong obfuscated, ulong value)
        {
            return new ObfuscatedULong(obfuscated.Value + value);
        }

        public static ObfuscatedULong operator +(ulong value, ObfuscatedULong obfuscated)
        {
            return new ObfuscatedULong(value + obfuscated.Value);
        }

        public static ObfuscatedULong operator -(ObfuscatedULong obfuscated1, ObfuscatedULong obfuscated2)
        {
            return new ObfuscatedULong(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedULong operator -(ObfuscatedULong obfuscated, ulong value)
        {
            return new ObfuscatedULong(obfuscated.Value - value);
        }

        public static ObfuscatedULong operator -(ulong value, ObfuscatedULong obfuscated)
        {
            return new ObfuscatedULong(value - obfuscated.Value);
        }

        public static ObfuscatedULong operator *(ObfuscatedULong obfuscated1, ObfuscatedULong obfuscated2)
        {
            return new ObfuscatedULong(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedULong operator *(ObfuscatedULong obfuscated, ulong value)
        {
            return new ObfuscatedULong(obfuscated.Value * value);
        }

        public static ObfuscatedULong operator *(ulong value, ObfuscatedULong obfuscated)
        {
            return new ObfuscatedULong(value * obfuscated.Value);
        }

        public static ObfuscatedULong operator /(ObfuscatedULong obfuscated1, ObfuscatedULong obfuscated2)
        {
            return new ObfuscatedULong(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedULong operator /(ObfuscatedULong obfuscated, ulong value)
        {
            return new ObfuscatedULong(obfuscated.Value / value);
        }

        public static ObfuscatedULong operator /(ulong value, ObfuscatedULong obfuscated)
        {
            return new ObfuscatedULong(value / obfuscated.Value);
        }

        public static ObfuscatedULong operator ++(ObfuscatedULong obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1);
            return obfuscated;
        }

        public static ObfuscatedULong operator --(ObfuscatedULong obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1);
            return obfuscated;
        }
    }
}
