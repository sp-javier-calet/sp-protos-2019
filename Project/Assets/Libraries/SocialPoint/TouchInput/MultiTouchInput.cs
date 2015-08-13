using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Touch
{
    public class MultiTouchInput
    {
        private const string MouseWheelAxis = "Mouse ScrollWheel";
        private static TouchPoint mouseTouch = new TouchPoint();
        private static TouchPoint mouseTouch2;
        private static float mouseWheelDistanceFactor = 0.01f;

        public static bool MouseWheelEnabled { get; set; }

        public static float movedDistance = 1f;

        public static float MovedDistance
        {
            get { return movedDistance; }
            set { movedDistance = value; }
        }

        private static List<int> _cancelledTouches = new List<int>();
        private static TouchPoint[] _touches = new TouchPoint[0];

        public static TouchPoint[] touches
        {
            get{ return _touches; }
        }

        public static void Update()
        {
            UpdateTouches();
        }

        private static void UpdateTouches()
        {
            int ntouches = Input.touches.Length;

            if(ntouches > 0)
            {
                _touches = new TouchPoint[ntouches];

                for(int i = 0; i < Input.touches.Length; ++i)
                {
                    _touches[i] = Input.touches[i];
                }

                UpdateCancelledTouches();
            }
            else
            {
                Vector2 screen = new Vector2(Screen.width, Screen.height);
                float mouseWheel = MouseWheelEnabled ? Input.GetAxis(MouseWheelAxis) : 0.0f;

                mouseTouch.deltaPosition = (Vector2)Input.mousePosition - mouseTouch.position;
                mouseTouch.deltaTime = Time.deltaTime;
                mouseTouch.fingerId = 0;
                mouseTouch.phase = TouchPhase.Ended;
                mouseTouch.position = (Vector2)Input.mousePosition;
                mouseTouch.tapCount = 1;
                mouseTouch.cancelled = false;
                mouseTouch.NormalizeDeltaPosition();

                if(Input.GetMouseButtonDown(0))
                {
                    ntouches++;
                    mouseTouch.phase = TouchPhase.Began;
                }
                else if(Input.GetMouseButton(0))
                {
                    ntouches++;
                    mouseTouch.phase = TouchPhase.Moved;
                }
                else if(Input.GetMouseButtonUp(0))
                {
                    ntouches++;
                    mouseTouch.phase = TouchPhase.Ended;
                }

                bool mouseTouch2Ended = false;

                mouseTouch2 = new TouchPoint(mouseTouch);
                mouseTouch2.fingerId++;
                mouseTouch2.deltaTime = Time.deltaTime;
                if(mouseWheel != 0)
                {
                    ntouches = 2;
                    mouseTouch.deltaPosition = new Vector2(mouseWheel, 0f);
                    Vector2 dist = mouseWheelDistanceFactor * screen;
                    mouseTouch.position.x += dist.x;
                    mouseTouch2.position.x -= dist.x;
                    mouseTouch.phase = TouchPhase.Moved;
                    mouseTouch2.deltaPosition = -1 * mouseTouch.deltaPosition;
                    mouseTouch2.phase = TouchPhase.Moved;
                }
                else if(Input.GetKey(KeyCode.Z) && ntouches > 0)
                {
                    ntouches++;
                    mouseTouch2.position = screen - mouseTouch.position;
                    mouseTouch2.deltaPosition.x *= -1f;
                    mouseTouch2.deltaPosition.y *= -1f;
                    mouseTouch2.phase = mouseTouch.phase;
                }
                else if(Input.GetKeyUp(KeyCode.Z) && ntouches > 0)
                {
                    ntouches++;
                    mouseTouch2.position = screen - mouseTouch.position;
                    mouseTouch2.deltaPosition.x *= -1f;
                    mouseTouch2.deltaPosition.y *= -1f;
                    mouseTouch2.phase = TouchPhase.Ended;
                    mouseTouch2Ended = true;
                }
                mouseTouch2.NormalizeDeltaPosition();

                if(mouseTouch.phase == TouchPhase.Moved && mouseTouch.deltaPosition.magnitude < movedDistance)
                {
                    mouseTouch.phase = TouchPhase.Stationary;

                    if(!mouseTouch2Ended)
                    {
                        mouseTouch2.phase = TouchPhase.Stationary;
                    }
                }

                _touches = new TouchPoint[ntouches];
                if(ntouches > 0)
                {
                    _touches[0] = mouseTouch;
                }
                if(ntouches > 1)
                {
                    _touches[1] = mouseTouch2;
                }

                UpdateCancelledTouches();
            }
        }

        static void UpdateCancelledTouches()
        {
            for(int i = 0; i < _touches.Length; ++i)
            {
                if(_touches[i].phase == TouchPhase.Began)
                {
                    _cancelledTouches.Remove(_touches[i].fingerId);
                }
                else if(_cancelledTouches.Contains(touches[i].fingerId))
                {
                    _touches[i].cancelled = true;
                }
            }
        }

        public static void CancelTouch(int id)
        {
            _cancelledTouches.Add(id);
        }
    }
}
