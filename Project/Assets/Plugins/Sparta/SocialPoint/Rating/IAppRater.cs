using System;

namespace SocialPoint.Rating
{
    public enum RateRequestResult
    {
        Decline,
        Accept,
        Delay
    }

    public delegate int GetUserLevelDelegate();

    public interface IAppRater
    {
        GetUserLevelDelegate GetUserLevel{ set; }

        void ShowRateView();

        void IncrementUsesCounts(bool canPromptForRating);

        void IncrementEventCounts(bool canPromptForRating);

        void ResetStatistics();

        void OnRequestResult(RateRequestResult result);

        event Action OnRequestResultAction;
    }
}

