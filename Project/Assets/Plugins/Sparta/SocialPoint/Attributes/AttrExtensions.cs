using System;

namespace SocialPoint.Attributes
{
    public static class AttrExtensions
    {
        public static void DoIfNotNull(this Attr val, Action<Attr> action)
        {
            if(val == null || action == null)
            {
                return;
            }

            action(val);
        }
    }
}