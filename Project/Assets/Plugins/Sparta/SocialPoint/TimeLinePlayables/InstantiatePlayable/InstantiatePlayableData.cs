using System;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
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

        public TimelineClip CustomClipReference { get; set; }
        public double CustomClipStart
        {
            get
            {
                return CustomClipReference.start;
            }
        }

        public double CustomClipEnd
        {
            get
            {
                return CustomClipReference.end;
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

            var baseCanvas = Parent.GetComponentInParent<Canvas>();
            if(baseCanvas != null)
            {
                if(PrefabIs3DObject)
                {
                    var uiViewController = Parent.GetComponentInParent<UIViewController>();
                    if(uiViewController != null)
                    {
                        uiViewController.Add3DContainer(InstantiatedPrefab);                      
                    }
                }
                else
                {
                    InstantiatedPrefab.SetLayerRecursively(baseCanvas.gameObject.layer);
                }
            }
        }

        public void DestroyOrRecycle()
        {
            if(InstantiatedPrefab != null)
            {
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