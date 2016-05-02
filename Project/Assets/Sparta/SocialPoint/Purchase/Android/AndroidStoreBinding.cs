using UnityEngine;
using System.Collections;

namespace SocialPoint.Purchase
{
    #if UNITY_ANDROID
    public class AndroidStoreBinding
    {
        private const string FullClassName = "es.socialpoint.sparta.purchase.SPPurchaseStore";

        private static AndroidJavaClass _notifClass = null;

        public static void Init(string listenerObjectName)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                if(Application.platform == RuntimePlatform.Android)
                {
                    _notifClass = new AndroidJavaClass(FullClassName);
                    _notifClass.CallStatic("Init", listenerObjectName);
                }
            }
        }

        public static void RequestProductData(string[] productIdentifiers)
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                _notifClass.CallStatic("RequestProductData", listenerObjectName);
            }
        }
    }
    #endif
}
