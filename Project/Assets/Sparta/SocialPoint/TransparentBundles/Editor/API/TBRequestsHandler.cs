using UnityEditor;
using System.Collections;


namespace SocialPoint.TransparentBundles
{
    [InitializeOnLoad]
    public static class WebRequestQueueHandler
    {

        static WebRequestQueueHandler()
        {
            MainThreadQueue.Instance.OnItemQueued += HandleQueuedItem;
        }


        static void HandleQueuedItem(object obj)
        {
            if(obj is AsyncRequestState)
            {
                var response = (AsyncRequestState)obj;

                response.RaiseCallback();
            }
        }
    }
}
