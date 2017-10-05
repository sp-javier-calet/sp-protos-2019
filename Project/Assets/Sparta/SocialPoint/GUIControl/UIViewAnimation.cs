using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation : ScriptableObject, ICloneable
    {
        public abstract float Duration { get; }

        public abstract void Load(UIViewController ctrl);

        public abstract IEnumerator Animate();

        public abstract void Reset();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}