using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public abstract class AdminPanelGUI
    {
        public abstract void OnCreateGUI(AdminPanelLayout layout);
    }

    public sealed class AdminPanelGUIOptions
    {
        public static readonly AdminPanelGUIOptions None = new AdminPanelGUIOptions();
    }

    public sealed class AdminPanelLayout : IDisposable
    {
        public Transform Parent { get; private set; }
        private float _offset;

        private Vector3 _currentPosition;
        public Vector3 Position
        {
            get { return _currentPosition;}
        }

        public AdminPanelLayout(Transform parent)
        {
            _currentPosition = new Vector3();
            Parent = parent;
        }

        public void Advance(float size)
        {
            _currentPosition.y -= size;
        }

        public void Dispose()
        {

        }
    }
}