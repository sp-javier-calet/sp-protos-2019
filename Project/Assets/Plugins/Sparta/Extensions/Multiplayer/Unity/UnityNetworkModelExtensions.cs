using SocialPoint.Physics;
using SocialPoint.Utils;
using SocialPoint.Exporter;
using Jitter.LinearMath;
using System.Collections.Generic;

using UnityGameObject = UnityEngine.GameObject;
using UnityTransform = UnityEngine.Transform;
using MultiplayerTransform = SocialPoint.Multiplayer.Transform;
using UnityCollider = UnityEngine.Collider;
using MultiplayerCollider = SocialPoint.Multiplayer.SceneCollider;

namespace SocialPoint.Multiplayer
{
    public static class UnityNetworkModelExtensions
    {
        public static MultiplayerTransform ToMultiplayer(this UnityTransform t)
        {
            return new MultiplayerTransform(
                t.position.ToPhysics(),
                t.rotation.ToPhysics(),
                t.localScale.ToPhysics()
            );
        }

        public static SceneCollider ToMultiplayer(this UnityCollider c)
        {
            return new SceneCollider {
                Id = c.name,
                Tags = new TagSet(c.transform.GetExportTags()),
                Transform = c.transform.ToMultiplayer(),
                Shape = c.ToPhysics()
            };
        }

        public static SceneTransform ToMultiplayerSceneTransform(this UnityTransform t)
        {
            var st = new SceneTransform {
                Id = t.name,
                Tags = new TagSet(t.GetExportTags()),
                Transform = t.ToMultiplayer(),
                Children = new List<Transform>()
            };
            var itr = t.GetEnumerator();
            while(itr.MoveNext())
            {
                var child = itr.Current as UnityTransform;
                st.Children.Add(child.ToMultiplayer());
            }
            return st;
        }
    }
}
