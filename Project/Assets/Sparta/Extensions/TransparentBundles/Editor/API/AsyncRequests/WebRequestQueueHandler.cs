using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    [InitializeOnLoad]
    public static class WebRequestQueueHandler
    {
        /// <summary>
        /// Class that observes the MainThreadQueue and handles AsyncRequestStates for firing their callbacks
        /// </summary>
        static WebRequestQueueHandler()
        {
            MainThreadQueue.Instance.OnItemDequeued += HandleQueuedItem;
        }

        /// <summary>
        /// If the object is of type AsyncRequestState raises its callback.
        /// </summary>
        /// <param name="obj">Dequeued object</param>
        static void HandleQueuedItem(object obj)
        {
            if(obj is AsyncRequestData)
            {
                var response = (AsyncRequestData)obj;

                response.RaiseCallback();
            }
        }
    }
}
