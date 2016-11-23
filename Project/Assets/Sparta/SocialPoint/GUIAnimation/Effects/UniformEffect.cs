using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class UniformEffect : BlendEffect
    {
        public enum UniformValueType
        {
            Float,
            Integer,
            Vector
        }

        [System.Serializable]
        public class UniformValues
        {
            [SerializeField]
            public UniformValueType ValueType = UniformValueType.Float;

            [SerializeField]
            public string UniformName = "_Blend";

            [SerializeField]
            public int IntStartValue = 1;
            [SerializeField]
            public int IntEndValue = 1;

            [SerializeField]
            public float FloatStartValue = 1f;
            [SerializeField]
            public float FloatEndValue = 1f;

            [SerializeField]
            public Vector4 VectorStartValue;
            [SerializeField]
            public Vector4 VectorEndValue;

            public void Blend(float blend, Material mat)
            {
                switch(ValueType)
                {
                case UniformValueType.Float:
                    float floatValue = Mathf.Lerp(FloatStartValue, FloatEndValue, blend);
                    mat.SetFloat(UniformName, floatValue);
                    break;

                case UniformValueType.Integer:
                    int intValue = Mathf.RoundToInt(Mathf.Lerp(IntStartValue, IntEndValue, blend));
                    mat.SetInt(UniformName, intValue);
                    break;

                case UniformValueType.Vector:
                    Vector4 vectorValue = Vector4.Lerp(VectorStartValue, VectorEndValue, blend);
                    mat.SetVector(UniformName, vectorValue);
                    break;
                }
            }

            public void Copy(UniformValues other)
            {
                UniformName = other.UniformName;
                ValueType = other.ValueType;

                IntStartValue = other.IntStartValue;
                IntEndValue = other.IntEndValue;

                FloatStartValue = other.FloatStartValue;
                FloatEndValue = other.FloatEndValue;

                VectorStartValue = other.VectorStartValue;
                VectorEndValue = other.VectorEndValue;
            }

            public void Invert()
            {
                float tempEndValue = FloatEndValue;
                FloatEndValue = FloatStartValue;
                FloatStartValue = tempEndValue;

                int tempEndValueInt = IntEndValue;
                IntEndValue = IntStartValue;
                IntStartValue = tempEndValueInt;

                Vector4 tempEndValueVector = VectorEndValue;
                VectorEndValue = VectorStartValue;
                VectorStartValue = tempEndValueVector;
            }

            public void SaveFromMaterial(Material mat, float normTime)
            {
                if(!mat.HasProperty(UniformName))
                {
                    return;
                }

                switch(ValueType)
                {
                case UniformValueType.Float:
                    float valFloat = FloatStartValue = mat.GetFloat(UniformName);
                    if(normTime < 0.5f)
                    {
                        FloatStartValue = valFloat;
                    }
                    else
                    {
                        FloatEndValue = valFloat;
                    }
                    break;
						
                case UniformValueType.Integer:
                    int valInt = mat.GetInt(UniformName);
                    if(normTime < 0.5f)
                    {
                        IntStartValue = valInt;
                    }
                    else
                    {
                        IntEndValue = valInt;
                    }
                    break;
						
                case UniformValueType.Vector:
                    Vector4 valVect = mat.GetVector(UniformName);
                    if(normTime < 0.5f)
                    {
                        VectorStartValue = valVect;
                    }
                    else
                    {
                        VectorEndValue = valVect;
                    }
                    break;
                }
            }
        }

        [ShowInEditor]
        [SerializeField]
        List<UniformValues> _values = new List<UniformValues>();

        public List<UniformValues> Values { get { return _values; } set { _values = value; } }

        IGraphicObject _graphicObject;

        IGraphicObject GraphicObject
        {
            get
            {
                if(Target == null)
                {
                    Log.d("There is no target");
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

        Material Material
        {
            get
            {
                if(GraphicObject == null)
                {
                    Log.d("There is no graphic object");
                    return null;
                }

                return GraphicObject.Material;
            }
        }

        public override void Copy(Step other)
        {
            base.Copy(other);

            CopyActionValues((UniformEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            var otherUnimEffect = (UniformEffect)other;

            _values.Clear();
            for(int i = 0; i < otherUnimEffect.Values.Count; ++i)
            {
                var effect = new UniformValues();
                effect.Copy(otherUnimEffect.Values[i]);
                _values.Add(effect);
            }
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
            for(int i = 0; i < Values.Count; ++i)
            {
                Values[i].Invert();
            }
        }

        public override void OnBlend(float blend)
        {
            if(Material == null)
            {
                Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                return;
            }

            for(int i = 0; i < Values.Count; ++i)
            {
                DoBlendUniform(Values[i], blend);
            }

            if(!Application.isPlaying)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }

        void DoBlendUniform(UniformValues uniform, float blend)
        {
            if(Material.HasProperty(uniform.UniformName))
            {
                uniform.Blend(blend, Material);
				
                GraphicObject.Refresh();
            }
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            if(Material == null)
            {
                return;
            }

            for(int i = 0; i < Values.Count; ++i)
            {
                Values[i].SaveFromMaterial(Material, localTimeNormalized);
            }
        }

        public int CreateUniforms(int numUniforms)
        {
            for(int i = 0; i < numUniforms; ++i)
            {
                _values.Add(new UniformValues());
            }

            return _values.Count;
        }

        public int RemoveUniforms(int numUniforms)
        {
            for(int i = 0; i < numUniforms; ++i)
            {
                if(_values.Count > 0)
                {
                    _values.RemoveAt(_values.Count - 1);
                }
            }
            return _values.Count;
        }
    }
}
