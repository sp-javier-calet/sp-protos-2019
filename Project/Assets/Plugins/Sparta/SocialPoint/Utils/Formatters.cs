namespace SocialPoint.Utils
{
    public static class Formatters
    {
        const string oneKFormatStr = "{0:0.00}K";
        const string oneMillionFormatStr = "{0:0.00}M";

        const float oneKMinimumValue = 10000f;
        const float oneMMinimumValue = 10000000f;

        public static string ResourceAmount(long amount)
        {
            string formatStr = "{0}";
            float finalAmount = amount;

            if (amount > oneKMinimumValue)
            {
                finalAmount = amount/1000f;
                formatStr = oneKFormatStr;
            }

            if (amount > oneMMinimumValue)
            {
                finalAmount = amount/10000000f;
                formatStr = oneMillionFormatStr;
            }

            return string.Format(formatStr, finalAmount);
        }
    }
}
