using System;
using SocialPoint.Base;

namespace SocialPoint.IO
{
    public class ShortEncoding
    {
        const int _maxInteger = 512;
        
        // 10 bits integer, 6 bits fractional
        public static short Encode(float value)
        {
            DebugUtils.Assert(value < _maxInteger, string.Format("[ShortEncoding] Value: {0} is Bigger than MaxInteger: {1}", (int) value, _maxInteger));

            value += value < 0 ? -1 : 1;
            var iv = (int)value;
            var fv = value - iv;
            var fi = Math.Abs((int)(fv * 64f));

            var res = iv << 6;
            res |= fi;
            return (short)res;
        }

        public static float Decode(short encoded)
        {
            var fi = encoded & 0x3f;
            var fv = ((float)fi) / 64f;
            var iv = encoded >> 6;
            var result = (float)iv + Math.Sign(iv) * fv;
            result += result < 0 ? +1 : -1;
            return result;
        }
    }
}