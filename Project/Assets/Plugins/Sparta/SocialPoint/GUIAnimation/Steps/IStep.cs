
namespace SocialPoint.GUIAnimation
{
    public enum AnimTimeMode
    {
        Global,
        Local
    }

    public interface IStep
    {
        float GetStartTime(AnimTimeMode mode);

        void SetStartTime(float time, AnimTimeMode mode);

        float GetEndTime(AnimTimeMode mode);

        void SetEndTime(float time, AnimTimeMode mode);

        float GetDuration(AnimTimeMode mode);

        void SetDuration(float duration, AnimTimeMode mode);

        void Invert(bool invertTime = false);
    }
}
