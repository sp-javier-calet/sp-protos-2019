using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    public interface INetworkInterpolate
    {
        bool Enable{ get; set; }

        void OnServerTransform(Transform t, float serverTimestamp);

        void OnNewObject(Transform t);

        JVector ServerPosition { get; }

        JQuaternion ServerRotation { get; }
    }
}
