using UnityEngine;
using SocialPoint.Physics;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public class NetworkMonoBehaviour : MonoBehaviour
    {
        NetworkGameObject _go;

        public void Init(NetworkGameObject go)
        {
            _go = go;
        }

        void Start()
        {
            SyncTransform();
        }

        void Update()
        {
            #if UNITY_EDITOR
            if(_go == null)
            {
                return;
            }
            #endif

            SyncTransform();
        }

        void SyncTransform()
        {
            if(_go != null)
            {
                transform.position = _go.Transform.Position.ToUnity();
                transform.rotation = _go.Transform.Rotation.ToUnity();
            }
        }
    }
}
