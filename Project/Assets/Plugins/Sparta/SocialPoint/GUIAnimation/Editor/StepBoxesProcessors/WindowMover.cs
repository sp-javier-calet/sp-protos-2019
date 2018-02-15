using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class WindowMover
    {
        public delegate void DeltaMomement(Vector2 delta);

        public Vector2 GrabSize = Vector2.zero;

        Vector2 _prevMousePos;

        bool _isMoving;

        public bool IsMoving { get { return _isMoving; } }

        public Vector2 Delta;

        public WindowMover(Vector2 grabSize)
        {
            GrabSize = grabSize;
        }

        public void Stop()
        {
            _isMoving = false;
        }

        public void Update(ref Rect window, DeltaMomement callback = null)
        {
            Rect expandedWindow = window;
            expandedWindow.size += GrabSize;

            if(Event.current.type == EventType.MouseUp)
            {
                _isMoving = false;
                return;
            }

            if(!_isMoving && Event.current.type == EventType.MouseDown &&
               expandedWindow.Contains(Event.current.mousePosition))
            {
                _isMoving = true;
                Delta = Vector2.zero;
                _prevMousePos = Event.current.mousePosition;
            }

            if(_isMoving)
            {
                Vector2 currentMousePos = Event.current.mousePosition;
                Delta = Event.current.mousePosition - _prevMousePos;
                window.position += Delta;

                _prevMousePos = currentMousePos;

                if(callback != null)
                {
                    callback(Delta);
                }
            }
        }
    }
}
