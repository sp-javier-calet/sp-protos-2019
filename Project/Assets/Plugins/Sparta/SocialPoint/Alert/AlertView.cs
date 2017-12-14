
namespace SocialPoint.Alert
{
    public sealed class AlertView
    #if UNITY_IOS && !UNITY_EDITOR
    : IosAlertView
    #elif UNITY_ANDROID && !UNITY_EDITOR
    : AndroidAlertView
    #else
    : UnityAlertView
    #endif
    {
    }
}