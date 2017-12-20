using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public sealed class UI3DContainer : MonoBehaviour
    {
        public event Action<GameObject> OnDestroyed;

        void OnDestroy()
        {
            if(OnDestroyed != null)
            {
                OnDestroyed(gameObject);
            }
        }
    }
}