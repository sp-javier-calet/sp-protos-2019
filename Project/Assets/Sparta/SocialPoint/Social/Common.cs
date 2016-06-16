using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
	public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);
}
