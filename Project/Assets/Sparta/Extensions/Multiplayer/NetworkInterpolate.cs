namespace SocialPoint.Multiplayer
{
    public interface INetworkInterpolate
    {
        bool Enable{ get; set; }

        void OnServerTransform(Transform t, float serverTimestamp);
    }
}
