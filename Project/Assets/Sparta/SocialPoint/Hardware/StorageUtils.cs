using System;

namespace SocialPoint.Hardware
{
    public enum StorageUnit
    {
        Bytes,
        KiloBytes,
        MegaBytes,
        GigaBytes,
    }

    public static class StorageUtils
    {
        public static ulong TransformStorageUnit(ulong amount, StorageUnit current, StorageUnit target)
        {
            int shift = ((int)current - (int)target) * 10;
            if(shift > 0)
            {
                amount <<= shift;
            }
            else
            {
                amount >>= Math.Abs(shift);
            }
            return amount;
        }
    }
}
