using System;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class InstantiatePlayableData : BasePlayableData
    {
        public GameObject Prefab;
        public GameObject InstantiatedPrefab;
        public Transform Parent;
        public bool UsePooling;
        public bool IsInstantiated;
        public bool PrefabIs3DObject;
        public Vector3 LocalPosition = Vector3.zero;
        public Quaternion LocalRotation = Quaternion.identity;
        public Vector3 LocalScale = Vector3.one;

        Canvas _baseCanvas;
        public Canvas BaseCanvas
        {
            get
            {
                if(_baseCanvas == null && Parent != null)
                {
                    _baseCanvas = Parent.GetComponentInParent<Canvas>();
                }

                return _baseCanvas;
            }
        }

        UIViewController _uiViewController;
        public UIViewController UIViewController
        {
            get
            {
                if(_uiViewController == null && Parent != null)
                {
                    _uiViewController = Parent.GetComponentInParent<UIViewController>();
                }

                return _uiViewController;
            }
        }

        public override void OnGraphStop(Playable playable)
        {
            if(!Application.isPlaying)
            {
                DestroyOrRecycle();
            }
        }

        public void InstantiateOrSpawn()
        {
            if(UsePooling)
            {
                InstantiatedPrefab = Prefab.Spawn(Parent);
            }
            else
            {
                InstantiatedPrefab = UnityEngine.Object.Instantiate(Prefab, Parent);
            }

            if(InstantiatedPrefab != null)
            {
                var trans = InstantiatedPrefab.transform;
                trans.localPosition = LocalPosition;
                trans.localRotation = LocalRotation;
                trans.localScale = LocalScale;
            }

            if(PrefabIs3DObject)
            {
                if(UIViewController != null)
                {
                    UIViewController.Add3DContainer(InstantiatedPrefab);
                }
            }
            // TODO add new layering system to 2d elements
            //else if(BaseCanvas != null)
            //{
            //    InstantiatedPrefab.SetLayerRecursively(BaseCanvas.gameObject.layer);
            //}
        }

        public void DestroyOrRecycle()
        {
            if(InstantiatedPrefab != null)
            {
                if(UIViewController != null && PrefabIs3DObject)
                {
                    UIViewController.On3dContainerDestroyed(InstantiatedPrefab);
                }

                if(UsePooling)
                {
                    InstantiatedPrefab.Recycle();
                }
                else
                {
                    InstantiatedPrefab.DestroyAnyway();
                }
            }
        }
    }
}