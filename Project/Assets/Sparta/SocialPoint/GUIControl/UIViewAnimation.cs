using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation : ScriptableObject, ICloneable
    {
        public abstract void Load(UIViewController ctrl);

        public abstract IEnumerator Animate();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}