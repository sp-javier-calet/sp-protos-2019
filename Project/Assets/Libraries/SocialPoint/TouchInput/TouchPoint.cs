using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Touch
{
    public class TouchPoint
    {
        public Vector2 deltaPosition;
        public float deltaTime;
        public int fingerId;
        public TouchPhase phase;
        public Vector2 position;
        public int tapCount;
        public bool cancelled;
        public Vector2 normalizedDeltaPosition;

        public TouchPoint()
        {
        }

        public TouchPoint(TouchPoint t)
        {
            deltaPosition = t.deltaPosition;
            deltaTime = t.deltaTime;
            fingerId = t.fingerId;
            phase = t.phase;
            position = t.position;
            tapCount = t.tapCount;
            cancelled = t.cancelled;
            NormalizeDeltaPosition();
        }

        public static implicit operator TouchPoint(UnityEngine.Touch touch)
        {
            TouchPoint t = new TouchPoint{
                deltaPosition = touch.deltaPosition,
                deltaTime = touch.deltaTime,
                fingerId = touch.fingerId,
                phase = touch.phase,
                position = touch.position,
                tapCount = touch.tapCount,

            };
            t.NormalizeDeltaPosition();
            return t;
        }

        public void NormalizeDeltaPosition()
        {
            //There are some problems with some IOS devices (mini ipad retina) where Touch.deltaTime = 0
            normalizedDeltaPosition = deltaPosition;
            if(deltaTime > 0.0f)
            {            
                normalizedDeltaPosition *= Time.deltaTime / deltaTime;
            }
        }
    }
}