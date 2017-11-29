
namespace SocialPoint.Components
{
    public enum BattleStep
    {
        None,
        Setup,
        Start,
        Update,
        Cleanup,
    }

    public enum BattleSetupState
    {
        Processing,
        Success,
    }

    public interface IBattleSetup
    {
        void Start();

        BattleSetupState Update(float dt);
    }

    public interface IBattleUpdate
    {
        void Update(float dt);
    }

    public interface IBattleCleanup
    {
        void Cleanup();
    }

    public interface IBattleStopListener
    {
        void OnStopped(bool successful);
    }

    public interface IBattleStop
    {
        void Stop();

        void RegisterListener(IBattleStopListener listener);

        void UnregisterListener(IBattleStopListener listener);
    }

    public interface IBattleStart
    {
        void Start();
    }

    public interface IBattleErrorHandler
    {
        void OnError(BattleError battleError);
    }

    public interface IBattleErrorDispatcher
    {
        void RegisterHandler(IBattleErrorHandler handler);
    }

    public class BattleError
    {
        public SocialPoint.Base.Error Error;
        public string Title;
        public string Description;
    }
}
