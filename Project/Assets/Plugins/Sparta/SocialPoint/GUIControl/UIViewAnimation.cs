using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public interface UIViewAnimation
    {
        void Load(GameObject gameObject);
        IEnumerator Animate();
    }

    public abstract class UIViewAnimationFactory : ScriptableObject
    {
        public abstract UIViewAnimation Create();
    }
}