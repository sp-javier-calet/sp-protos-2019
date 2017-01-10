using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class ColorEffect : BlendEffect
    {
        public sealed class TargetValueMonitor : StepMonitor
        {
            public Color Color;

            public override void Backup()
            {
                IGraphicObject graphic = GetGraphicObject();
                if(graphic != null)
                {
                    Color = graphic.Color;
                }
            }

            IGraphicObject GetGraphicObject()
            {
                return GraphicObjectLoader.Load(Target, true);
            }

            public override bool HasChanged()
            {
                Color original = Color;

                Color newColor = original;
                IGraphicObject widget = GetGraphicObject();
                if(widget != null)
                {
                    newColor = widget.Color;
                }

                return Math.Abs(newColor.r - Color.r) > Single.Epsilon || Math.Abs(newColor.g - Color.g) > Single.Epsilon || Math.Abs(newColor.b - Color.b) > Single.Epsilon;
            }
        }

        [SerializeField]
        [ShowInEditor]
        Color _startValue = Color.white;

        public Color StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        [ShowInEditor]
        Color _endValue = Color.white;

        public Color EndValue { get { return _endValue; } set { _endValue = value; } }

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
            CopyActionValues((ColorEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _startValue = ((ColorEffect)other).StartValue;
            _endValue = ((ColorEffect)other).EndValue;
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

            Color tempEndValue = _endValue;
            _endValue = _startValue;
            _startValue = tempEndValue;
        }

        public override void OnBlend(float blend)
        {
            if(TargetWidget == null)
            {
                if(Animation != null && Animation.EnableWarnings)
                {
                    Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                }
                return;
            }

            TargetWidget.Color = Color.Lerp(StartValue, EndValue, blend);

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
                StartValue = TargetWidget.Color;
            }
            else
            {
                EndValue = TargetWidget.Color;
            }
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}
