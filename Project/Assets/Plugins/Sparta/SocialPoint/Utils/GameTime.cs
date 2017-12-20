namespace SocialPoint.Utils
{
    public interface IGameTime
    {
        float Time { get; }

        float UnscaledTime { get; }

        float DeltaTime{ get; }

        float Scale{ get; set; }

        int FrameCount{ get; }
    }

    public class GameTime : IDeltaUpdateable, IGameTime
    {
        public float UnscaledTime { get; private set; }

        public float Time { get; private set; }

        public float DeltaTime { get; private set; }

        public float Scale { get; set; }

        public int FrameCount { get; private set; }

        public GameTime()
        {
            UnscaledTime = 0f;
            Time = 0f;
            DeltaTime = 0f;
            Scale = 1f;
            FrameCount = 0;
        }

        public void Update(float dt)
        {
            DeltaTime = dt;
            UnscaledTime += dt;
            Time += dt * Scale;
            FrameCount++;
        }
    }
}