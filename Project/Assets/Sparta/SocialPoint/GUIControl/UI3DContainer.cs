using UnityEngine;
using System;

namespace SocialPoint.GUIControl
{
    public class UI3DContainer : MonoBehaviour
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