using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation : ScriptableObject, ICloneable
    {
        [SerializeField]
        protected Transform _transform;

        public virtual void Load(Transform transform = null)
        {
            if(transform == null)
            {
                throw new MissingComponentException("Missing Transform component in UIViewAnimation Load");
            }

            if(_transform == null)
            {
                _transform = transform;
            }
        }

        public abstract IEnumerator Animate();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}