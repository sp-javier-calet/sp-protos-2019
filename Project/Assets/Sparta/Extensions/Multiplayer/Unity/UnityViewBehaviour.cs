﻿using UnityEngine;
using SocialPoint.Physics;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public sealed class UnityViewBehaviour : NetworkBehaviour
    {
        class NetworkMonoBehaviour : MonoBehaviour
        {
            NetworkGameObject _go;

            public void Init(NetworkGameObject go)
            {
                _go = go;
            }

            void Start()
            {
                SyncTransform();
            }

            void Update()
            {
#if UNITY_EDITOR
                #warning TODO : Remove after testing
                var skillManager = _go.GetBehaviour<SkillManagerBehaviour>();
                if(skillManager != null)
                {
                    if(Input.GetKeyDown("c"))
                    {
                        for(var itr = skillManager.Skills.GetEnumerator(); itr.MoveNext();)
                        {
                            var action = new SkillAction {
                                Object = _go.Id,
                                SkillSlot = itr.Current.Slot,
                                ActionType = SkillActionType.Cancel,
                                Target = 0
                            };
                            skillManager.SceneController.ApplyAction(action);
                        }
                    }
                    else if(Input.GetKey("i"))
                    {
                        for(var itr = skillManager.Skills.GetEnumerator(); itr.MoveNext();)
                        {
                            var action = new SkillAction {
                                Object = _go.Id,
                                SkillSlot = itr.Current.Slot,
                                ActionType = SkillActionType.Interrupt,
                                Target = 0
                            };
                            skillManager.SceneController.ApplyAction(action);
                        }
                    }
                    else if(Input.GetKeyUp("p"))
                    {
                        var controller = _go.GetBehaviour<HeroPlayerControllerBehaviour>();
                        if (controller != null && controller.IsControlledByPlayer)
                        {
                            var action = new AddStatusEffectAction {
                                StatusEffectId = "Stun",
                                StatusEffectLevel = 1,
                                Objects = new List<int>() {_go.Id}
                            };
                            skillManager.SceneController.ApplyAction(action);
                        }
                    }
                    else if(Input.GetKeyUp("j"))
                    {
                        var controller = _go.GetBehaviour<HeroPlayerControllerBehaviour>();
                        if (controller != null && controller.IsControlledByPlayer)
                        {
                            var dmg = new DamageFormula.Result();
                            dmg.TrueDamage = 1000000;

                            var action = new DamageEvent {
                                AttackerUnitKind = _go.GetBehaviour<UnitKindBehaviour>().UnitKind,
                                AttackerChampionIndex = _go.GetBehaviour<HeroBehaviour>().ChampionData.ChampionIndex,
                                AttackerId = _go.Id,
                                Damage = dmg,
                                DamagedObjects = new List<int>() {_go.Id},
                                Attacker = _go,
                                LifeSteal = 0
                            };
                            skillManager.SceneController.ApplyAction(action);
                        }
                    }
                    else if(Input.GetKeyUp("n"))
                    {
                        var controller = _go.GetBehaviour<HeroPlayerControllerBehaviour>();
                        if (controller != null && controller.IsControlledByPlayer)
                        {
                            var action = new AddStatusEffectAction {
                                StatusEffectId = "Immunity",
                                StatusEffectLevel = 1,
                                Objects = new List<int>() {_go.Id}
                            };
                            skillManager.SceneController.ApplyAction(action);
                        }
                    }

                }
                // End TODO
#endif

                SyncTransform();
            }

            void SyncTransform()
            {
                if(_go != null)
                {
                    transform.position = _go.Transform.Position.ToUnity();
                    transform.rotation = _go.Transform.Rotation.ToUnity();
                }
            }
        }

        GameObject _view;

        public GameObject View
        {
            get
            {
                if(_view == null)
                {
                    InstantiateGameObject();
                }
                return _view;
            }
        }

        const string _prefabsBasePath = "Prefabs/";
        string _prefabName;

        UnityViewPool _viewPool;

        System.Func<NetworkGameObject, string> _obtainPrefabNameCallback;

        void InstantiateGameObject()
        {
            var prefabName = _obtainPrefabNameCallback == null ? _prefabName : _obtainPrefabNameCallback(GameObject);
            _view = _viewPool.Spawn(prefabName,
                GameObject.Transform.Position.ToUnity(),
                GameObject.Transform.Rotation.ToUnity());

            var networkMonoBehaviour = _view.GetComponent<NetworkMonoBehaviour>();
            if(networkMonoBehaviour == null)
            {
                networkMonoBehaviour = _view.AddComponent<NetworkMonoBehaviour>();
            }
            networkMonoBehaviour.Init(GameObject);
        }

        public UnityViewBehaviour Init(System.Func<NetworkGameObject, string> obtainPrefabNameCallback, UnityViewPool viewPool)
        {
            _viewPool = viewPool;
            _obtainPrefabNameCallback = obtainPrefabNameCallback;
            return this;
        }

        public UnityViewBehaviour Init(string prefabName, UnityViewPool viewPool)
        {
            _prefabName = prefabName;
            _viewPool = viewPool;
            return this;
        }

        protected override void OnStart()
        {
            if(_view == null)
            {
                InstantiateGameObject();
            }
        }

        protected override void OnDestroy()
        {
            DestroyView();
        }

        protected override void Dispose()
        {
            DestroyView();

            _viewPool = null;
            _obtainPrefabNameCallback = null;

            base.Dispose();
        }

        void DestroyView()
        {
            if(_view != null)
            {
                _viewPool.Recycle(_view);
                _view = null;
            }
        }

        public void SetHeroViewType(string heroType)
        {
            string heroPrefabName = heroType.ToLower();
            _prefabName = _prefabsBasePath + heroPrefabName;
        }

        public override object Clone()
        {
            var vb = GameObject.Context.Pool.Get<UnityViewBehaviour>();
            vb.Init(_prefabName, _viewPool);
            vb._obtainPrefabNameCallback = _obtainPrefabNameCallback;
            return vb;
        }
    }
}
