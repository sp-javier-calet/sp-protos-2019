using System.Runtime.InteropServices;

namespace SocialPoint.Utils.Obfuscation
{
    public class ObfuscatedInt : Obfuscated<int>
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct UnionMaskInt
        {
            [FieldOffset(0)]
            public int value;
            [FieldOffset(0)]
            public ulong mask;
        }

        public ObfuscatedInt(int value = default(int))
            : base(value)
        {
        }

        protected override int Obfuscate(int value)
        {
            UnionMaskInt reinterpret = default(UnionMaskInt);
            reinterpret.value = value;
            DoObfuscate(ref reinterpret.mask);
            
            return reinterpret.value;
        }

        public static ObfuscatedInt operator +(ObfuscatedInt obfuscated1, ObfuscatedInt obfuscated2)
        {
            return new ObfuscatedInt(obfuscated1.Value + obfuscated2.Value);
        }

        public static ObfuscatedInt operator +(ObfuscatedInt obfuscated, int value)
        {
            return new ObfuscatedInt(obfuscated.Value + value);
        }

        public static ObfuscatedInt operator +(int value, ObfuscatedInt obfuscated)
        {
            return new ObfuscatedInt(value + obfuscated.Value);
        }

        public static ObfuscatedInt operator -(ObfuscatedInt obfuscated1, ObfuscatedInt obfuscated2)
        {
            return new ObfuscatedInt(obfuscated1.Value - obfuscated2.Value);
        }

        public static ObfuscatedInt operator -(ObfuscatedInt obfuscated, int value)
        {
            return new ObfuscatedInt(obfuscated.Value - value);
        }

        public static ObfuscatedInt operator -(int value, ObfuscatedInt obfuscated)
        {
            return new ObfuscatedInt(value - obfuscated.Value);
        }

        public static ObfuscatedInt operator *(ObfuscatedInt obfuscated1, ObfuscatedInt obfuscated2)
        {
            return new ObfuscatedInt(obfuscated1.Value * obfuscated2.Value);
        }

        public static ObfuscatedInt operator *(ObfuscatedInt obfuscated, int value)
        {
            return new ObfuscatedInt(obfuscated.Value * value);
        }

        public static ObfuscatedInt operator *(int value, ObfuscatedInt obfuscated)
        {
            return new ObfuscatedInt(value * obfuscated.Value);
        }

        public static ObfuscatedInt operator /(ObfuscatedInt obfuscated1, ObfuscatedInt obfuscated2)
        {
            return new ObfuscatedInt(obfuscated1.Value / obfuscated2.Value);
        }

        public static ObfuscatedInt operator /(ObfuscatedInt obfuscated, int value)
        {
            return new ObfuscatedInt(obfuscated.Value / value);
        }

        public static ObfuscatedInt operator /(int value, ObfuscatedInt obfuscated)
        {
            return new ObfuscatedInt(value / obfuscated.Value);
        }

        public static ObfuscatedInt operator ++(ObfuscatedInt obfuscated)
        {
            obfuscated.Set(obfuscated.Value + 1);
            return obfuscated;
        }

        public static ObfuscatedInt operator --(ObfuscatedInt obfuscated)
        {
            obfuscated.Set(obfuscated.Value - 1);
            return obfuscated;
        }
    }
}
