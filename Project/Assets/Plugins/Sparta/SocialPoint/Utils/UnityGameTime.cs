
namespace SocialPoint.Utils
{
    public class UnityGameTime : IGameTime
    {
        public float UnscaledTime { get { return UnityEngine.Time.unscaledTime; } }

        public float Time { get { return UnityEngine.Time.time; } }

        public float DeltaTime { get { return UnityEngine.Time.deltaTime; } }

        public float Scale { get { return UnityEngine.Time.timeScale; } set { UnityEngine.Time.timeScale = value; } }

        public int FrameCount { get { return UnityEngine.Time.frameCount; } }
    }
}