using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation<T> : ScriptableObject, ICloneable
    {
        public abstract void Load(T ctrl);

        public abstract IEnumerator Appear();

        public abstract IEnumerator Disappear();

        public abstract void Reset();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}