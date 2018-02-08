using System;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class InstantiatePlayableData : BasePlayableData
    {
        public GameObject Prefab;
        public GameObject InstantiatedPrefab;
        public Transform Parent;
        public bool UsePooling;
        public bool IsInstantiated;
        public bool IsParticleSystem;
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
            if(Application.isPlaying && UsePooling)
            {
                InstantiatedPrefab = Prefab.Spawn(Parent);
            }
            else
            {
                InstantiatedPrefab = UnityEngine.Object.Instantiate(Prefab, Parent);
            }

            if(InstantiatedPrefab != null)
            {
                // TODO At some point, when we can hace both 2D and 3D objects in the same popup we can remove this if is not needed
                if(IsParticleSystem)
                {
                    InstantiatedPrefab.SetLayerRecursively(Parent.gameObject.layer);

                    // Hack - We need to setup the scaling mode to be in "Hierarchy" if we want to scale the
                    // ParticleSystems correctly in our UI.
                    //
                    // The new ParticleSystems that tech art are developing/updating, will have "Hierarchy" as default 
                    // and at that point we can remove this Hack. Until then we need to live with this if we want to adapt 
                    // particles to UI correctly
                    var particleSystems = InstantiatedPrefab.GetComponentsInChildren<ParticleSystem>(true);
                    if(particleSystems.Length > 0)
                    {
                        for(int i = 0; i < particleSystems.Length; ++i)
                        {
                            var particle = particleSystems[i];
                            if(particle != null)
                            {
                                var main = particle.main;
                                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                            }
                        }
                    }
                }

                var trans = InstantiatedPrefab.transform;
                trans.localPosition = LocalPosition;
                trans.localRotation = LocalRotation;
                trans.localScale = LocalScale;
            }

            if(Application.isPlaying && UIViewController != null)
            {
                UIViewController.Add3DContainer(InstantiatedPrefab);
            }
        }

        public void DestroyOrRecycle()
        {
            if(InstantiatedPrefab != null)
            {
                if(Application.isPlaying && UIViewController != null)
                {
                    UIViewController.On3dContainerDestroyed(InstantiatedPrefab);
                }

                if(Application.isPlaying && UsePooling)
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