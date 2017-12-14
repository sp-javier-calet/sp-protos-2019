using SocialPoint.AppEvents;

namespace SocialPoint.Restart
{
    public interface IRestarter
    {
        void RestartGame();
    }

    public class DefaultRestarter : IRestarter
    {
        readonly IAppEvents _appEvents;

        public DefaultRestarter(IAppEvents appEvents)
        {
            _appEvents = appEvents;
        }

        #region IRestarter implementation

        public void RestartGame()
        {
            _appEvents.RestartGame(0);
        }

        #endregion
    
    }
}
