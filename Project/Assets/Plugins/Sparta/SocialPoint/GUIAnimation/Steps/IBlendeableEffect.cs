using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.GUIAnimation
{
    public interface IBlendeableEffect
    {
        bool UseEaseCustom { get; set; }

        List<EasePoint> EaseCustom { get; set; }

        EaseType EaseType { get; set; }
    }
}
