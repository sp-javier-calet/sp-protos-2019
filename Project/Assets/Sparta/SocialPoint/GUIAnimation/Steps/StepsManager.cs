using System.Collections.Generic;

// This class stores all the Step information and is extendable to allow adding more effects
namespace SocialPoint.GUIAnimation
{
    public sealed class StepData
    {
        public System.Type StepType;
        public bool ConfigurableInEditor;
        public bool AutoAddOnChange;
    }

    public sealed class StepMonitorData
    {
        public System.Type StepType;
        public System.Type StepMonitorType;
    }

    public static class StepsManager
    {
        public static List<StepData> BlendStepsData = new List<StepData> {
            new StepData {
                StepType = typeof(AnchorsEffect),
                ConfigurableInEditor = true
            },
            new StepData {
                StepType = typeof(PositionEffect),
                ConfigurableInEditor = true
            },
            new StepData { StepType = typeof(ScaleEffect) },
            new StepData { StepType = typeof(RotationEffect) },

            new StepData { StepType = typeof(ColorEffect) },
            new StepData { StepType = typeof(OpacityEffect) },
            new StepData {
                StepType = typeof(UniformEffect),
                ConfigurableInEditor = true
            },

            // Deprecated
            new StepData {
                StepType = typeof(TransformEffect),
                ConfigurableInEditor = true
            },
        };

        public static StepData GetBlendStepData(System.Type type)
        {
            return BlendStepsData.Find(adata => adata.StepType == type);
        }

        public static List<StepMonitorData> BlendMonitorsData = new List<StepMonitorData> {
            new StepMonitorData {
                StepType = typeof(TransformEffect),
                StepMonitorType = typeof(TransformEffect.TargetValueMonitor)
            },

            new StepMonitorData {
                StepType = typeof(AnchorsEffect),
                StepMonitorType = typeof(AnchorsEffect.TargetValueMonitor)
            },
            new StepMonitorData {
                StepType = typeof(PositionEffect),
                StepMonitorType = typeof(PositionEffect.TargetValueMonitor)
            },
            new StepMonitorData {
                StepType = typeof(ScaleEffect),
                StepMonitorType = typeof(ScaleEffect.TargetValueMonitor)
            },
            new StepMonitorData {
                StepType = typeof(RotationEffect),
                StepMonitorType = typeof(RotationEffect.TargetValueMonitor)
            },

            new StepMonitorData {
                StepType = typeof(ColorEffect),
                StepMonitorType = typeof(ColorEffect.TargetValueMonitor)
            },
            new StepMonitorData {
                StepType = typeof(OpacityEffect),
                StepMonitorType = typeof(OpacityEffect.TargetValueMonitor)
            },
        };

        public static StepMonitorData GeMonitorData(System.Type monitorType)
        {
            return BlendMonitorsData.Find(adata => adata.StepMonitorType == monitorType);
        }

        public static List<StepData> TriggerStepsData = new List<StepData> {
            new StepData { StepType = typeof(ParticleSpawnerEffect) },
            new StepData { StepType = typeof(GameObjectEnablerEffect) },
            new StepData { StepType = typeof(ParticlePlayerEffect) },
            new StepData { StepType = typeof(ParticleStopperEffect) },
            new StepData { StepType = typeof(CallbackEffect) },
            new StepData { StepType = typeof(AnimatorEffect) },
        };

        public static StepData GetInstantStepData(System.Type type)
        {
            return TriggerStepsData.Find(adata => adata.StepType == type);
        }

        public static Dictionary<System.Type, string> StepsNames = new Dictionary<System.Type, string> {
            // Abstract Composite Types
            { typeof(Group), "Group" },
            { typeof(EffectsGroup), "Transition" },

            // Abstract Effect Types
            { typeof(Effect), "Effect" },
            { typeof(BlendEffect), "Blend Effect" },
            { typeof(TriggerEffect), "Trigger Effect" },

            // Blending Effects
            { typeof(AnchorsEffect), "Anchors" },
            { typeof(PositionEffect), "Position" },
            { typeof(ScaleEffect), "Scale" },
            { typeof(RotationEffect), "Rotation" },
            { typeof(TransformEffect), "Transform" },

            { typeof(ColorEffect), "Color" },
            { typeof(OpacityEffect), "Opacity" },
            { typeof(UniformEffect), "Uniform" },

            // Triggers Effects
            { typeof(ParticleSpawnerEffect), "Particle Spawner" },
            { typeof(GameObjectEnablerEffect), "Object Enabler" },
            { typeof(ParticlePlayerEffect), "Particle Player" },
            { typeof(ParticleStopperEffect), "Particle Stopper" },
            { typeof(CallbackEffect), "Callback" },
            { typeof(AnimatorEffect), "Animator" },
        };

        public static string GetStepName(System.Type type)
        {
            string name = type.ToString();
            StepsNames.TryGetValue(type, out name);
            return name;
        }

        //---- Methods to extend the steps and stepsMonitors
        public static void AddBlendingStepData(System.Type blendingStepType, string visibleName)
        {
            AddStepData(StepsManager.BlendStepsData, blendingStepType, visibleName);
        }

        public static void AddTriggerStepData(System.Type triggerStepType, string visibleName)
        {
            AddStepData(StepsManager.TriggerStepsData, triggerStepType, visibleName);
        }

        static void AddStepData(List<StepData> steps, System.Type stepType, string visibleName)
        {
            if(steps.Exists(astep => astep.StepType == stepType))
            {
                return;
            }
            steps.Add(new StepData { StepType = stepType });

            visibleName = !string.IsNullOrEmpty(visibleName) ? visibleName : stepType.ToString();
            StepsNames.Add(stepType, visibleName);
        }

        public static void AddMonitorStepData(System.Type stepType, System.Type stepMonitorType)
        {
            if(StepsManager.BlendMonitorsData.Exists(astep => astep.StepType == stepType))
            {
                return;
            }
            StepsManager.BlendMonitorsData.Add(new StepMonitorData {
                StepType = stepType,
                StepMonitorType = stepMonitorType
            });
        }
    }
}
