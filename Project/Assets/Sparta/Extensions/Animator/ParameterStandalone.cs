using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Animations
{
    public class ParameterStandalone
    {
        public bool Dirty;

        int _intValue;

        public int IntValue
        {
            get
            {
                return _intValue;
            }
            set
            {
                Dirty |= _intValue != value;
                _intValue = value;
            }
        }

        float _floatValue;

        public float FloatValue
        {
            get
            {
                return _floatValue;
            }
            set
            {
                Dirty |= (Math.Abs(_floatValue - value) > 1e-5f);
                _floatValue = value;
            }
        }

        bool _boolValue;

        public bool BoolValue
        {
            get
            {
                return _boolValue;
            }
            set
            {
                Dirty = true;
                TriggerUserValue = _boolValue = value;
            }
        }

        public bool TriggerUserValue{ get; set; }

        public ParameterDataType Type
        {
            get
            {
                return _data.Type;
            }
        }

        public int DefaultInt
        {
            get
            {
                return _data.DefaultInt;
            }
        }

        public float DefaultFloat
        {
            get
            {
                return _data.DefaultFloat;
            }
        }

        public bool DefaultBool
        {
            get
            {
                return _data.DefaultBool;
            }
        }

        ParameterData _data;

        public ParameterData data{ get { return _data; } }

        public ParameterStandalone(ParameterData data)
        {
            _data = data;
            ResetValue();
        }

        public void ResetTrigger()
        {
            _boolValue = _data.DefaultBool;
        }

        public void ResetValue()
        {
            IntValue = _data.DefaultInt;
            FloatValue = _data.DefaultFloat;
            BoolValue = data.DefaultBool;
        }
    }
}
