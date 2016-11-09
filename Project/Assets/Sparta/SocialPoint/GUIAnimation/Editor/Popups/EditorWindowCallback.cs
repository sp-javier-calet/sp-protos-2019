using System;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
    public class EditorWindowCallback : EditorWindow
    {
        Action _onAccept;
        Action _onCancel;

        public virtual void SetCallbacks(Action onAccept, Action onCancel)
        {
            _onAccept = onAccept;
            _onCancel = onCancel;
        }

        public virtual void SetCallbacks(Action onAccept)
        {
            _onAccept = onAccept;
        }

        public virtual void OnAccept()
        {
            if(_onAccept != null)
            {
                _onAccept();
            }
        }

        public virtual void OnCancel()
        {
            if(_onCancel != null)
            {
                _onCancel();
            }
        }

        public void Update()
        {
            // Make the window always on top
            EditorWindow.FocusWindowIfItsOpen<EditorWindowCallback>();
        }
    }
}
