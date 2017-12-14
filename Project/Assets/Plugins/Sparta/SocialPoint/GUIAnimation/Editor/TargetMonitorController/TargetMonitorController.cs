using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    public sealed class TargetMonitorController
    {
        public interface IMonitorController
        {
            bool Monitor(List<StepMonitorData> modifiedData);

            void Backup();
        }

        //-------
        public class EffectTargetMonitorController : IMonitorController
        {
            Effect _step;
            readonly MultiTargetValueMonitor _monitor = new MultiTargetValueMonitor();

            public EffectTargetMonitorController(Effect step)
            {
                _step = step;
                _monitor.Init(_step.Target, _step.CreateTargetMonitor());
                _monitor.Backup();
            }

            public bool Monitor(List<StepMonitorData> modifiedData)
            {
                if(_step == null)
                {
                    return false;
                }
                
                bool hasChanged = _monitor.HasChanged();
                _monitor.Backup();
                if(hasChanged)
                {
                    modifiedData.Add(StepsManager.GeMonitorData(_monitor.MonitorType));
                }
                return hasChanged;
            }

            public void Backup()
            {
                _monitor.Backup();
            }
        }
        //------

        //-------
        public class EffectsGroupTargetMonitorController : IMonitorController
        {
            EffectsGroup _step;
            readonly List<MultiTargetValueMonitor> _monitors = new List<MultiTargetValueMonitor>();

            public EffectsGroupTargetMonitorController(EffectsGroup step)
            {
                _step = step;

                for(int i = 0; i < StepsManager.BlendMonitorsData.Count; ++i)
                {
                    var monitor = new MultiTargetValueMonitor();
                    monitor.Init(_step.Targets, StepsManager.BlendMonitorsData[i].StepMonitorType);
                    _monitors.Add(monitor);
                    monitor.Backup();
                }
            }

            public bool Monitor(List<StepMonitorData> modifiedData)
            {
                bool somethingChanged = false;
                for(int i = 0; i < _monitors.Count; ++i)
                {
                    bool hasChanged = _monitors[i].HasChanged();
                    _monitors[i].Backup();
                    somethingChanged |= hasChanged;

                    if(hasChanged)
                    {
                        StepMonitorData monitorData = StepsManager.GeMonitorData(_monitors[i].MonitorType);
                        modifiedData.Add(monitorData);

                        StepData stepData = StepsManager.GetBlendStepData(monitorData.StepType);
                        if(stepData.AutoAddOnChange)
                        {
                            _step.AddActionType<Effect>(monitorData.StepType);
                        }
                    }
                }

                return somethingChanged;
            }

            public void Backup()
            {
                for(int i = 0; i < _monitors.Count; ++i)
                {
                    _monitors[i].Backup();
                }
            }
        }
        //------

        IMonitorController monitorController;

        public void Init(Step step)
        {
            monitorController = null;

            var effect = step as Effect;
            if(effect != null)
            {
                monitorController = new EffectTargetMonitorController(effect);
            }
            else {
                var effectsGroup = step as EffectsGroup;
                if(effectsGroup != null)
                {
                    monitorController = new EffectsGroupTargetMonitorController(effectsGroup);
                }
            }
        }

        public bool Monitor(List<StepMonitorData> modifiedData)
        {
            modifiedData.Clear();

            return monitorController != null && monitorController.Monitor(modifiedData);
        }

        public void Backup()
        {
            if(monitorController != null)
            {
                monitorController.Backup();
            }
        }

        public void Reset()
        {
            monitorController = null;
        }
    }
}
