using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class OpacityEffect : BlendEffect
    {
        public sealed class TargetValueMonitor : StepMonitor
        {
            public float Alpha;

            public override void Backup()
            {
                IGraphicObject widget = GetWidget();
                if(widget != null)
                {
                    Alpha = widget.Alpha;
                }
            }


            IGraphicObject GetWidget()
            {
                return GraphicObjectLoader.Load(Target, true);
            }

            public override bool HasChanged()
            {
                float original = Alpha;

                float newAlpha = original;
                IGraphicObject widget = GetWidget();
                if(widget != null)
                {
                    newAlpha = widget.Alpha;
                }

                return Math.Abs(newAlpha - Alpha) > Single.Epsilon;
            }
        }

        [SerializeField]
        [ShowInEditor]
        float _startValue = 1f;

        public float StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        [ShowInEditor]
        float _endValue = 1f;

        public float EndValue { get { return _endValue; } set { _endValue = value; } }

        IGraphicObject _graphicObject;

        IGraphicObject TargetWidget
        {
            get
            {
                if(Target == null)
                {
                    return null;
                }

                if(Application.isPlaying && _graphicObject != null)
                {
                    return _graphicObject;
                }

                _graphicObject = GraphicObjectLoader.Load(Target, true);
                return _graphicObject;
            }
        }

        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyActionValues((OpacityEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _startValue = ((OpacityEffect)other).StartValue;
            _endValue = ((OpacityEffect)other).EndValue;
        }

        public override void OnRemoved()
        {
        }

        public override void SetOrCreateDefaultValues()
        {
            SaveValuesAt(0f);
            SaveValuesAt(1f);
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            float tempEndValue = _endValue;
            _endValue = _startValue;
            _startValue = tempEndValue;
        }

        public override void OnBlend(float blend)
        {
            if(TargetWidget == null)
            {
                Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                return;
            }

            TargetWidget.Alpha = Mathf.Lerp(StartValue, EndValue, blend);

            if(!Application.isPlaying)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            if(TargetWidget == null)
            {
                return;
            }
			
            if(localTimeNormalized < 0.5f)
            {
                StartValue = TargetWidget.Alpha;
            }
            else
            {
                EndValue = TargetWidget.Alpha;
            }
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}
