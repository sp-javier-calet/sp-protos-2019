using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedDouble : Obfuscated<double>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskDouble
        {
            [FieldOffset(0)]
            public double value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public static implicit operator ObfuscatedDouble(double value)
        {
            return new ObfuscatedDouble(value);
        }

        public ObfuscatedDouble(double value = default(double))
            : base(value)
        {
        }

        protected override ulong Obfuscate(double value)
        {
            UnionMaskDouble reinterpret = default(UnionMaskDouble);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.mask;
        }

        protected override double Unobfuscate(ulong value)
        {
            UnionMaskDouble reinterpret = default(UnionMaskDouble);
            reinterpret.mask = value;
            DoObfuscate(ref reinterpret.mask);

            return reinterpret.value;
        }

        public static ObfuscatedDouble operator +(ObfuscatedDouble obfuscated1, ObfuscatedDouble obfuscated2)
        {
            return new ObfuscatedDouble(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedDouble operator +(ObfuscatedDouble obfuscated, double value)
        {
            return new ObfuscatedDouble(obfuscated.Value + value);
        }

        public static ObfuscatedDouble operator +(double value, ObfuscatedDouble obfuscated)
        {
            return new ObfuscatedDouble(value + obfuscated.Value);
        }

        public static ObfuscatedDouble operator -(ObfuscatedDouble obfuscated1, ObfuscatedDouble obfuscated2)
        {
            return new ObfuscatedDouble(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedDouble operator -(ObfuscatedDouble obfuscated, double value)
        {
            return new ObfuscatedDouble(obfuscated.Value - value);
        }

        public static ObfuscatedDouble operator -(double value, ObfuscatedDouble obfuscated)
        {
            return new ObfuscatedDouble(value - obfuscated.Value);
        }

        public static ObfuscatedDouble operator *(ObfuscatedDouble obfuscated1, ObfuscatedDouble obfuscated2)
        {
            return new ObfuscatedDouble(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedDouble operator *(ObfuscatedDouble obfuscated, double value)
        {
            return new ObfuscatedDouble(obfuscated.Value * value);
        }

        public static ObfuscatedDouble operator *(double value, ObfuscatedDouble obfuscated)
        {
            return new ObfuscatedDouble(value * obfuscated.Value);
        }

        public static ObfuscatedDouble operator /(ObfuscatedDouble obfuscated1, ObfuscatedDouble obfuscated2)
        {
            return new ObfuscatedDouble(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedDouble operator /(ObfuscatedDouble obfuscated, double value)
        {
            return new ObfuscatedDouble(obfuscated.Value / value);
        }

        public static ObfuscatedDouble operator /(double value, ObfuscatedDouble obfuscated)
        {
            return new ObfuscatedDouble(value / obfuscated.Value);
        }

        public static ObfuscatedDouble operator ++(ObfuscatedDouble obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1.0);
            return obfuscated;
        }

        public static ObfuscatedDouble operator --(ObfuscatedDouble obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1.0);
            return obfuscated;
        }
    }
}
