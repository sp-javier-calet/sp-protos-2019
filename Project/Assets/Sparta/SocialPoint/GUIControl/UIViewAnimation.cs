using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public abstract class UIViewAnimation : ScriptableObject, ICloneable
    {
        [SerializeField]
        protected GameObject _gameObject;
        protected Transform _transform;
        protected RectTransform _rectTransform;

        public virtual void Load(GameObject gameObject = null)
        {
            if(_gameObject == null)
            {
                _gameObject = gameObject;
                _transform = _gameObject.transform;
            }
                
            _rectTransform = _transform as RectTransform;
        }

        public abstract IEnumerator Animate();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}