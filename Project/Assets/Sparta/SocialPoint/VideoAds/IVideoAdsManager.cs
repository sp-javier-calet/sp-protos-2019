using SocialPoint.Base;

using System;

namespace SocialPoint.VideoAds
{
    public enum ShowVideoResult
    {
        Finished,
        Aborted,
        Error
    }

    public enum RequestVideoResult
    {
        Available,
        NotAvailable,
        Error
    }

    public delegate void ShowVideoDelegate(Error error,ShowVideoResult result);

    public delegate void RequestVideoDelegate(Error error,RequestVideoResult result);

    public interface IVideoAdsManager : IDisposable
    {
        /// <summary>
        /// Occurs when ad started.
        /// </summary>
        event Action AdStartedEvent;

        /// <summary>
        /// Occurs when ad finished.
        /// </summary>
        event Action AdFinishedEvent;

        /// <summary>
        /// Gets a value indicating whether is an ad available.
        /// </summary>
        /// <value><c>true</c> if ad available; otherwise, <c>false</c>.</value>
        bool AdAvailable { get; }

        /// <summary>
        /// Requests a video. 
        /// Acording Fyber guidelines videos should be requested only before showing it, otherwise
        /// you are burning requests and downloading useless data.
        /// </summary>
        /// <param name="cbk">Cbk.</param>
        void RequestAd(RequestVideoDelegate cbk = null);

        void ShowAd(ShowVideoDelegate cbk = null);

        bool IsEnabled { get; }

        void Enable();

        void Disable();

    }
}

