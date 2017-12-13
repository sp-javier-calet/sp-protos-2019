using UnityEngine;

namespace SocialPoint.Base
{
    public static class TouchExtension
    {
        public static Vector2 NormalizedDeltaPosition(this Touch touch)
        {
            //Checking the deltaTime is causing problems in some ios devices (ipad air 1). The deltaPosition is always normalized on ios.
            #if (UNITY_IOS || UNITY_TVOS)
                Vector2 normalizedDeltaPosition = touch.deltaPosition;
            #else
            Vector2 normalizedDeltaPosition = touch.deltaPosition;
            if(touch.deltaTime > 0.0f)
            {            
                normalizedDeltaPosition *= Time.deltaTime / touch.deltaTime;
            }
            #endif
            return normalizedDeltaPosition;
        }
    }
}