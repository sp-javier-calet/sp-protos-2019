using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Touch
{
    public interface ITouchAware
    {
        void OnTouchUp(TouchPoint touch, GameObject touched);

        void OnTouchMove(TouchPoint touch, GameObject touched);

        void OnTouchDown(TouchPoint touch, GameObject touched);
    }

    public interface ITouchable
    {
    }

    public struct TouchHit
    {
        public GameObject gameObject;
        public float distance;

        public TouchHit( GameObject obj, float dist )
        {
            gameObject = obj;
            distance = dist;
        }
    };
    
    public class TouchDispatcher
    {
        public delegate bool TouchEnabledDelegate(Vector3 pos);

        public TouchEnabledDelegate TouchEnabled;

        private float _rayCastRange;
        private int _layerMask;
        private Camera _camera;
        private MonoBehaviour _behaviour;
        private List<TouchHit> _hits;

        public Camera Camera
        {
            get
            {
                if(_camera == null)
                {
                    return Camera.main;
                }
                return _camera;
            }
            
            set
            {
                _camera = value;
            }
        }

        public IList<TouchHit> Hits
        {
            get
            {
                return _hits;
            }
        }
        
        public TouchDispatcher(MonoBehaviour behaviour, float rayCastRange = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers)
        {
            _behaviour = behaviour;
            _rayCastRange = rayCastRange;
            _layerMask = layerMask;

            _behaviour.StartCoroutine(UpdateCoroutine());
        }

        IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                Update();
                yield return true;
            }
        }

        public void Update()
        {
            MultiTouchInput.Update();
            for(int k = 0; k < MultiTouchInput.touches.Length; k++)
            {
                TouchPoint touch = MultiTouchInput.touches[k];
                GameObject touched;
                ITouchAware hitComp = GetNearestHit(touch.position, out touched);
                if(hitComp != null && !touch.cancelled)
                {
                    switch(touch.phase)
                    {
                    case TouchPhase.Began:
                        hitComp.OnTouchDown(touch, touched);
                        break;
                    case TouchPhase.Moved:
                        hitComp.OnTouchMove(touch, touched);
                        break;
                    case TouchPhase.Ended:
                        hitComp.OnTouchUp(touch, touched);
                        break;
                    }
                }
            }
        }

        private ITouchAware GetNearestHit(Vector3 pos, out GameObject touched)
        {
            if(TouchEnabled != null && !TouchEnabled(pos))
            {
                touched = null;
                return null;
            }

            Camera cam = Camera;
            if(cam == null)
            {
                touched = null;
                return null;
            }
            Ray ray = cam.ScreenPointToRay(pos);

            _hits = new List<TouchHit>();

            var hits3d = Physics.RaycastAll(ray, _rayCastRange, _layerMask);
            foreach(var hit in hits3d)
            {
                _hits.Add(new TouchHit(hit.collider.gameObject, hit.distance));
            }

            var hits2d = Physics2D.GetRayIntersectionAll(ray, _rayCastRange, _layerMask);
            foreach(var hit in hits2d)
            {
                _hits.Add(new TouchHit(hit.collider.gameObject, hit.distance));
            }
            _hits.Sort(delegate(TouchHit hit1, TouchHit hit2) { 
                return hit1.distance.CompareTo(hit2.distance);
            });

            foreach(var hit in _hits)
            {
                touched = hit.gameObject.GetParentWithInterfaceComponent<ITouchable>();
                if(touched)
                {
                    ITouchAware comp = touched.GetParentInterfaceComponent<ITouchAware>();
                    if(comp != null)
                    {
                        return comp;
                    }
                }
            }

            touched = null;
            return null;
        }
    }
    
}
