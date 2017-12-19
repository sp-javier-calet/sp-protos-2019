﻿using System;

namespace SocialPoint.Hardware
{
    public enum StorageUnit
    {
        Bytes,
        KiloBytes,
        MegaBytes,
        GigaBytes,
    }

    [Serializable]
    public struct StorageAmount
    {
        public ulong Amount;
        public StorageUnit Unit;

        public ulong Bytes
        {
            get
            {
                return ToAmount(StorageUnit.Bytes);
            }
        }

        public ulong ToAmount(StorageUnit target = StorageUnit.Bytes)
        {
            int shift = ((int)Unit - (int)target) * 10;
            var amount = Amount;
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

        public StorageAmount(ulong amount, StorageUnit unit = StorageUnit.Bytes)
        {
            Amount = amount;
            Unit = unit;
        }

        public StorageAmount Transform(StorageUnit target)
        {
            return new StorageAmount{
                Amount = ToAmount(target),
                Unit = target
            };
        }

        public override string ToString()
        {
            return string.Format("[{0} {1}]", Amount, Unit);
        }
    }
}
