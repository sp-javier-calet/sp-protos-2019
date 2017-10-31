using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

            // HINT: If we want to scale  a object with Canvas Scaler component on it we force to scale his first child
            var canvasScaler = _gameObject.GetComponent<CanvasScaler>();
            if(canvasScaler != null)
            {
                if(_transform.childCount > 0)
                {
                    _transform = _transform.GetChild(0);
                    _gameObject = _transform.gameObject;
                }
            }
                
            _rectTransform = _transform as RectTransform;
        }

        public abstract IEnumerator Animate();

        #region ICloneable implementation

        public abstract object Clone();

        #endregion
    }
}