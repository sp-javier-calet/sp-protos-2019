using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class EffectsChangesMonitor
    {
        public class EffectsGroup
        {
            public List<StepMonitor> Monitors = new List<StepMonitor>();

            public void Backup()
            {
                for(int i = 0; i < Monitors.Count; ++i)
                {
                    Monitors[i].Backup();
                }
            }

            public bool HasChanged()
            {
                bool changed = false;
                for(int i = 0; i < Monitors.Count; ++i)
                {
                    changed |= Monitors[i].HasChanged();
                }

                return changed;
            }

            public void Init(Transform target, List<Effect> actions)
            {
                Monitors.Clear();
                for(int i = 0; i < actions.Count; ++i)
                {
                    StepMonitor monitor = actions[i].CreateTargetMonitor();
                    if(monitor != null)
                    {
                        monitor.Init(target);
                        monitor.Backup();

                        Monitors.Add(monitor);
                    }
                }
            }
        }

        List<Transform> _targets = new List<Transform>();
        List<Effect> _actions = new List<Effect>();

        readonly List<EffectsGroup> _monitors = new List<EffectsGroup>();

        public void SetTargets(List<Transform> targets)
        {
            _monitors.Clear();
            _targets.Clear();

            for(int i = 0; i < targets.Count; ++i)
            {
                _targets.Add(targets[i]);
            }

            InitMonitors();
        }

        public void SetActions(List<Component> actions)
        {
            _monitors.Clear();
            _actions.Clear();

            for(int i = 0; i < actions.Count; ++i)
            {
                Component animItem = actions[i];
                var effect = animItem as Effect;
                if(effect != null)
                {
                    Effect action = effect;
                    if(!_actions.Exists(addedAction => addedAction.GetType() == action.GetType()))
                    {
                        _actions.Add(action);
                    }
                }
            }

            InitMonitors();
        }

        void InitMonitors()
        {
            _monitors.Clear();
            for(int i = 0; i < _targets.Count; ++i)
            {
                if(_targets[i] == null)
                {
                    continue;
                }

                var group = new EffectsGroup();
                group.Init(_targets[i], _actions);

                _monitors.Add(group);
            }
        }

        public bool Monitor()
        {
            bool someHasChanges = false;
            for(int i = 0; i < _monitors.Count; ++i)
            {
                someHasChanges |= _monitors[i].HasChanged();
                _monitors[i].Backup();
            }
            return someHasChanges;
        }

        bool IsValidTarget(Transform target)
        {
            return _targets.Contains(target);
        }

        public void ResetState()
        {
            _monitors.Clear();
            InitMonitors();
        }
    }
}
