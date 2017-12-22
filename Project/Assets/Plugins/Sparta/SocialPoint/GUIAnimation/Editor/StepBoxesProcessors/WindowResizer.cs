using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class WindowResizer
    {
        public delegate void OnResized(Vector2 delta);

        int _grabDistance;

        public int GrabDistance { get { return _grabDistance; } set { _grabDistance = value; } }

        bool _isResizing;

        public bool IsResizing { get { return _isResizing; } }

        Vector2 LastMousePos;
        Vector2 _minSize;

        public Vector2 DeltaSize;

        public WindowResizer(int grabDistance, Vector2 minSize)
        {
            _grabDistance = grabDistance;
            _minSize = minSize;
            _isResizing = false;
        }

        public void Stop()
        {
            _isResizing = false;
        }

        public void Resize(ref Rect ResizingWindow, Vector2 axis, OnResized callback = null)
        {
            if(ResizingWindow.Contains(Event.current.mousePosition) &&
               Event.current.type == EventType.MouseDown &&
               Mathf.Abs(Event.current.mousePosition.x - (ResizingWindow.position.x + ResizingWindow.width)) < _grabDistance &&
               Mathf.Abs(Event.current.mousePosition.y - (ResizingWindow.position.y + ResizingWindow.height)) < _grabDistance)
            {
                _isResizing = true;
                LastMousePos = Event.current.mousePosition;
            }
            else
                _isResizing &= Event.current.type != EventType.MouseUp;

            if(_isResizing)
            {
                Vector2 prevSize = ResizingWindow.size;

                ResizingWindow.width += (Event.current.mousePosition.x - LastMousePos.x) * axis.x;
                ResizingWindow.width = Mathf.Max(_minSize.x, ResizingWindow.width);

                ResizingWindow.height += (Event.current.mousePosition.y - LastMousePos.y) * axis.y;
                ResizingWindow.height = Mathf.Max(_minSize.y, ResizingWindow.height);

                LastMousePos = Event.current.mousePosition;

                DeltaSize = ResizingWindow.size - prevSize;
                if(callback != null)
                {
                    callback(DeltaSize);
                }
            }
        }
    }
}
