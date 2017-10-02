using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation : ScriptableObject, ICloneable
    {
        public abstract void Load(UIViewController ctrl);

        public abstract IEnumerator Appear();

        public abstract IEnumerator Disappear();

        public abstract void Reset();

        #region ICloneable implementation

        public virtual object Clone() 
        {
            return null;
        }

        #endregion
    }
}