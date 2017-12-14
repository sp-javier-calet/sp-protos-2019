using UnityEngine.SocialPlatforms;

namespace SocialPoint.Base
{
    public static class RangeExtensions
    {
        public static int RelativeCount(this Range range)
        {
            return range.from + range.count;
        }

        public static int Last(this Range range)
        {
            if(range.count > 0)
            {
                return range.RelativeCount() - 1;
            }

            return 0;
        }

        public static bool Contains(this Range range, int num)
        {
            return num >= range.from && num < range.RelativeCount();
        }
    }
}
