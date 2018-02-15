using System;

namespace SocialPoint.Matchmaking
{
    public class EmptyMatchmakingClient : BaseMatchmakingClient, IDisposable
    {
        public override void Start()
        {
            var match = new Match();
            TriggerOnMatched(match);
        }

        #region IDisposable implementation

        public void Dispose()
        {
        }

        #endregion
    }
}

