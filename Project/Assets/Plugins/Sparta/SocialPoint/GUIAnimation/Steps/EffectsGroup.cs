using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class EffectsGroup : Group, IBlendeableEffect
    {
        [SerializeField]
        GameObject _template;

        GameObject Template
        {
            get
            {
                SetOrCreateTemplate();
                return _template;
            }
        }

        [SerializeField]
        List<Transform> _targets = new List<Transform>();

        public List<Transform> Targets { get { return _targets; } }

        [SerializeField]
        bool _useEaseCustom = true;

        public bool UseEaseCustom { get { return _useEaseCustom; } set { _useEaseCustom = value; } }

        [SerializeField]
        List<EasePoint> _easeCustom = new List<EasePoint> {
            new EasePoint(0f, 0f),
            new EasePoint(1f, 1f)
        };

        public List<EasePoint> EaseCustom { get { return _easeCustom; } set { _easeCustom = value; } }

        [SerializeField]
        EaseType _easeType;

        public EaseType EaseType { get { return _easeType; } set { _easeType = value; } }

        public override void Refresh()
        {
            base.Refresh();
            SetOrCreateTemplate();
        }

        public override void Invert(bool invertTime)
        {
            base.Invert(invertTime);
            Easing.InvertCustom(_easeCustom);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            _targets.Clear();

            if(_template != null)
            {
                Object.DestroyImmediate(_template.gameObject);
                _template = null;
            }
        }

        public override void Copy(Step other)
        {
            base.Copy(other);

            // Copy Targets
            _targets = new List<Transform>(((EffectsGroup)other).Targets);

            // Copy Template
            _template = null;
            SetOrCreateTemplate();
            CopyTemplate((EffectsGroup)other);

            // Copy Easing
            _useEaseCustom = ((EffectsGroup)other).UseEaseCustom;
            _easeCustom = new List<EasePoint>(((EffectsGroup)other).EaseCustom);
            _easeType = ((EffectsGroup)other).EaseType;
        }

        public void CopyTemplate(EffectsGroup other)
        {
            List<Component> actionTemplates = other.GetActionTemplates();
            for(int i = 0; i < actionTemplates.Count; ++i)
            {
                Component copy = Template.AddComponent(actionTemplates[i].GetType());
                ((Step)copy).Animation = Animation;
                ((Step)copy).Copy((Step)actionTemplates[i]);
            }
        }

        public bool AddTarget(Transform iTarget)
        {
            bool existTarget = ContainsTarget(iTarget);
            if(!existTarget)
            {
                Targets.Add(iTarget);

                CreateAllActionsForTarget(iTarget);
                _animation.RefreshAndInit();

                // By default apply all the values
                CopySharedValuesToTarget(iTarget);
                OverrideEasingOfTarget(iTarget);

                return true;
            }
            return false;
        }

        public bool RemoveTarget(Transform iTarget)
        {
            List<Step> animItemsWithTarget = _animItems.FindAll(iAction => (iAction is Effect && ((Effect)iAction).Target == iTarget));

            // Remove all actions that contains the target
            for(int i = 0; i < animItemsWithTarget.Count; ++i)
            {
                animItemsWithTarget[i].OnRemoved();
                Object.DestroyImmediate(animItemsWithTarget[i]);

                _animItems.Remove(animItemsWithTarget[i]);
            }

            // Remove the target from targets list
            Targets.Remove(iTarget);

            _animation.RefreshAndInit();
            return true;
        }

        void CreateAllActionsForTarget(Transform iTarget)
        {
            List<Component> actionTemplates = GetActionTemplates();
            for(int i = 0; i < actionTemplates.Count; ++i)
            {
                int totalAnimItems = AnimItems.Count;

                System.Type actionType = actionTemplates[i].GetType();
                string newActionName = iTarget.name + " - " + StepsManager.GetStepName(actionType);
                var newAction = (Effect)AddAnimationItem(actionType, newActionName);

                SetupDefaultActionConfig(newAction, totalAnimItems);

                newAction.Target = iTarget;
                newAction.SetOrCreateDefaultValues();
            }
        }

        static void SetupDefaultActionConfig(Step action, int totalAnimItems)
        {
            action.SetStartTime(0f, AnimTimeMode.Local);
            action.SetEndTime(1f, AnimTimeMode.Local);
            action.SetSlot(totalAnimItems);
        }

        void CreateActionForAllTargets(System.Type actionType)
        {
            for(int i = 0; i < Targets.Count; ++i)
            {
                int totalAnimItems = AnimItems.Count;

                string newActionName = Targets[i].name + " - " + StepsManager.GetStepName(actionType);
                var action = (Effect)AddAnimationItem(actionType, newActionName);
                action.OnCreated();

                SetupDefaultActionConfig(action, totalAnimItems);

                action.Target = Targets[i];
                action.SetOrCreateDefaultValues();
            }
        }

        void RemoveAllActionOfType(System.Type actionType)
        {
            List<Step> animItemsOfType = _animItems.FindAll(iAction => (iAction.GetType() == actionType));
			
            // Remove all actions of that type
            for(int i = 0; i < animItemsOfType.Count; ++i)
            {
                animItemsOfType[i].OnRemoved();
                _animItems.Remove(animItemsOfType[i]);
                Object.DestroyImmediate(animItemsOfType[i]);
            }
        }

        public bool ContainsTarget(Transform iTarget)
        {
            return Targets.Contains(iTarget);
        }

        void SetOrCreateTemplate()
        {
            if(_template == null)
            {
                _template = AnchorUtility.CreateParentTransform("_Template" + StepName).gameObject;
                _template.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            }
			
            _template.transform.SetParent(ItemsRoot, false);
        }

        public T AddActionType<T>(System.Type actionType) where T:Effect
        {
            T newActionTemplate = default(T);
            bool exists = ContainsActionType<T>(actionType);
            if(!exists)
            {
                newActionTemplate = (T)Template.AddComponent(actionType);
                newActionTemplate.enabled = false;

                CreateActionForAllTargets(actionType);

                _animation.RefreshAndInit();
            }
			
            return newActionTemplate;
        }

        public bool RemoveActionType<T>(System.Type actionType) where T:Effect
        {
            T actionTemplate = GetActionTemplate<T>(actionType);
            if(actionTemplate != default(T))
            {
                RemoveAllActionOfType(actionType);

                Object.DestroyImmediate(actionTemplate);

                _animation.RefreshAndInit();

                return true;
            }

            return false;
        }

        public bool ContainsActionType<T>(System.Type actionType) where T:Effect
        {
            return GetActionTemplate<T>(actionType) != default(T);
        }

        public T GetActionTemplate<T>(System.Type actionType) where T:Effect
        {
            List<Component> actions = GetActionTemplates();
            return (T)actions.Find(i => i.GetType() == actionType);
        }

        public List<Component> GetActionTemplates()
        {
            var actionTemplates = new List<Component>(Template.GetComponents<Component>());
            for(int i = 0; i < actionTemplates.Count;)
            {
                if(!(actionTemplates[i] is Effect))
                {
                    actionTemplates.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return actionTemplates;
        }

        public bool OverrideAnimItemsByTemplate(System.Type type)
        {
            Effect atemplate = GetActionTemplate<Effect>(type);
            if(atemplate == null)
            {
                Log.w("Template: " + type.ToString() + " is not found");
                return false;
            }

            List<Step> animItemsOfType = AnimItems.FindAll(iAction => (iAction.GetType() == atemplate.GetType()));
            for(int animItemIdx = 0; animItemIdx < animItemsOfType.Count; ++animItemIdx)
            {
                ((Effect)animItemsOfType[animItemIdx]).CopyActionValues(atemplate);
            }

            return true;
        }

        void CopySharedValuesToTarget(Object target)
        {
            List<Step> animItemsOfTarget = AnimItems.FindAll(iAction => (iAction is Effect) && (((Effect)iAction).Target == target));
            for(int animItemIdx = 0; animItemIdx < animItemsOfTarget.Count; ++animItemIdx)
            {
                Effect template = GetActionTemplate<Effect>(animItemsOfTarget[animItemIdx].GetType());
                ((Effect)animItemsOfTarget[animItemIdx]).CopySharedValues(template);
            }
        }

        public void OverrideEasing()
        {
            for(int i = 0; i < AnimItems.Count; ++i)
            {
                ((BlendEffect)AnimItems[i]).CopyEasing(_useEaseCustom, _easeCustom, _easeType);
            }
        }

        void OverrideEasingOfTarget(Transform target)
        {
            for(int i = 0; i < AnimItems.Count; ++i)
            {
                ((BlendEffect)AnimItems[i]).CopyEasing(_useEaseCustom, _easeCustom, _easeType);
            }
        }
    }
}
