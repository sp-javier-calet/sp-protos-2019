using System;
using FixMath.NET;
using SocialPoint.NetworkModel;
using SocialPoint.Pooling;
using SocialPoint.Utils;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace Examples.Multiplayer.Lockstep
{
    public class CubeViewNetworkBehavior : INetworkBehaviour
    {
        static readonly Fix64 InstanceMinScale = (Fix64) 0.2f;
        static readonly Fix64 InstanceMaxScale = (Fix64) 2.0f;
        Transform _parent;
        GameObject _prefab;

        XRandom _random;
        GameObject _view;

        public NetworkGameObject GameObject { get; set; }

        void INetworkBehaviour.OnAdded()
        {
            var scale = new Vector3((float) _random.Range(InstanceMinScale, InstanceMaxScale), (float) _random.Range(InstanceMinScale, InstanceMaxScale), (float) _random.Range(InstanceMinScale, InstanceMaxScale));
            var position = new Vector3((float) GameObject.Transform.Position.x, (float) GameObject.Transform.Position.y * scale.y, (float) GameObject.Transform.Position.z);

            _view = UnityObjectPool.Spawn(_prefab, _parent, position, Quaternion.identity);
            _view.transform.localScale = scale;
        }

        void INetworkBehaviour.OnRemoved()
        {
            UnityObjectPool.Recycle(_view);
        }

        void INetworkBehaviour.OnObjectDestroyed()
        {
        }

        void IDisposable.Dispose()
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
        }

        public void Init(XRandom random, Transform parent, GameObject prefab)
        {
            _random = random;
            _parent = parent;
            _prefab = prefab;
        }
    }
}