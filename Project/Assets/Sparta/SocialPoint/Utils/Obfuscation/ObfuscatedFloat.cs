using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedFloat : Obfuscated<float>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskFloat
        {
            [FieldOffset(0)]
            public float value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedFloat(float value)
        {
            return new ObfuscatedFloat(value);
        }

        public ObfuscatedFloat(float value = default(float))
            : base(value)
        {
        }

        protected override ulong Obfuscate(float value)
        {
            UnionMaskFloat reinterpret = default(UnionMaskFloat);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override float Unobfuscate(ulong value)
        {
            UnionMaskFloat reinterpret = default(UnionMaskFloat);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedFloat operator +(ObfuscatedFloat obfuscated1, ObfuscatedFloat obfuscated2)
        {
            return new ObfuscatedFloat(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedFloat operator +(ObfuscatedFloat obfuscated, float value)
        {
            return new ObfuscatedFloat(obfuscated.Value + value);
        }

        public static ObfuscatedFloat operator +(float value, ObfuscatedFloat obfuscated)
        {
            return new ObfuscatedFloat(value + obfuscated.Value);
        }

        public static ObfuscatedFloat operator -(ObfuscatedFloat obfuscated1, ObfuscatedFloat obfuscated2)
        {
            return new ObfuscatedFloat(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedFloat operator -(ObfuscatedFloat obfuscated, float value)
        {
            return new ObfuscatedFloat(obfuscated.Value - value);
        }

        public static ObfuscatedFloat operator -(float value, ObfuscatedFloat obfuscated)
        {
            return new ObfuscatedFloat(value - obfuscated.Value);
        }

        public static ObfuscatedFloat operator *(ObfuscatedFloat obfuscated1, ObfuscatedFloat obfuscated2)
        {
            return new ObfuscatedFloat(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedFloat operator *(ObfuscatedFloat obfuscated, float value)
        {
            return new ObfuscatedFloat(obfuscated.Value * value);
        }

        public static ObfuscatedFloat operator *(float value, ObfuscatedFloat obfuscated)
        {
            return new ObfuscatedFloat(value * obfuscated.Value);
        }

        public static ObfuscatedFloat operator /(ObfuscatedFloat obfuscated1, ObfuscatedFloat obfuscated2)
        {
            return new ObfuscatedFloat(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedFloat operator /(ObfuscatedFloat obfuscated, float value)
        {
            return new ObfuscatedFloat(obfuscated.Value / value);
        }

        public static ObfuscatedFloat operator /(float value, ObfuscatedFloat obfuscated)
        {
            return new ObfuscatedFloat(value / obfuscated.Value);
        }

        public static ObfuscatedFloat operator ++(ObfuscatedFloat obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1.0f);
            return obfuscated;
        }

        public static ObfuscatedFloat operator --(ObfuscatedFloat obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1.0f);
            return obfuscated;
        }
    }
}
