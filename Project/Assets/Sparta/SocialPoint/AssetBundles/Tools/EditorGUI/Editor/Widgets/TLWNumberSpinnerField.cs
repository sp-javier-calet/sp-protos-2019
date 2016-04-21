using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A number field with a min and max value defined that can be incremented or decremented through + and - buttons.
    /// </summary>
    /// Numbers should be integers, although it is prepared to work with decimal(with minimal modifications).
    public class TLWNumberSpinnerField : TLWidget
    {
        public static readonly TLStyle              SpinnerFieldStyle;
        public static readonly TLStyle              SpinnerButtonStyle;

        public static readonly GUILayoutOption[]    SpinnerButtonOptions;

        static TLWNumberSpinnerField()
        {
            SpinnerFieldStyle = new TLStyle("TextField");
            SpinnerFieldStyle.margin = new RectOffset();
            SpinnerFieldStyle.alignment = TextAnchor.MiddleLeft;

            SpinnerButtonStyle = new TLStyle("Button");
            SpinnerButtonStyle.margin = new RectOffset();
            SpinnerButtonStyle.padding = new RectOffset(1,1,1,1);

            SpinnerButtonOptions = new GUILayoutOption[] { GUILayout.Width(18), GUILayout.Height(10) };
        }

        GUILayoutOption[]    SpinnerFieldOptions;
        protected string    _s_number;
        float               _maxWidgetLength;

        protected float _number;
        protected float _minNum;
        protected float _maxNum;
        protected float _step;

        /// <summary>
        /// Gets or sets the number as a string.
        /// </summary>
        /// <value>The number as a string.</value>
        public string number
        {
            get
            { 
                return _s_number;
            }
            set 
            {
                _number = Clamp(float.Parse(value));
                _s_number = _number.ToString("G");
            } 
        }

        float Clamp(float value)
        {
            if(value > _maxNum) return _maxNum;
            else if(value < _minNum) return _minNum;
            else return value;
        }

        public TLWNumberSpinnerField( TLView view, string name, GUILayoutOption[] options ) : base (view, name, options)
        {
            _minNum = 0f;
            _maxNum = 100f;
            number = _minNum.ToString("G");
            _step = 1f;
            Style = new TLStyle ("TextField");
            SpinnerFieldOptions = new GUILayoutOption[] { GUILayout.Height(20) };
        }

        public TLWNumberSpinnerField( TLView view, string name, int minNum, int maxNum, GUILayoutOption[] options ) : base (view, name, options)
        {
            _minNum = Mathf.Min((float)minNum, (float)maxNum);
            _maxNum = Mathf.Max((float)maxNum, (float)minNum);
            number = _minNum.ToString("G");
            _step = 1f;
            Style = new TLStyle ("TextField");
            SpinnerFieldOptions = new GUILayoutOption[] { GUILayout.Height(20) };
        }

        protected virtual void Increment()
        {
            var new_val = _number + _step;
            number = new_val.ToString("G");
        }

        protected virtual void Decrement()
        {
            var new_val = _number - _step;
            number = new_val.ToString("G");
        }

        public override void Perform()
        {
            //precalc
            if(_maxWidgetLength <= 0)
            {
                _maxWidgetLength = SpinnerFieldStyle.GetStyle().CalcSize(new GUIContent(_maxNum.ToString("G"))).x;
                SpinnerFieldOptions = new GUILayoutOption[] { GUILayout.Height(20), GUILayout.Width(_maxWidgetLength + 12f) };
            }

            GUI.SetNextControlName (Name);

            GUILayout.BeginHorizontal(Options);

            GUILayout.FlexibleSpace();

            //Number field
            var input = GUILayout.TextField( _s_number, 11, SpinnerFieldStyle.GetStyle(), SpinnerFieldOptions );

            //swallow line breaks
            input = input.Replace("\n",string.Empty);
            
            // int field
            float outVal;
            if (float.TryParse(input, out outVal)) {
                number = input;
            }

            //Buttons
            GUILayout.BeginVertical();

            if(GUILayout.Button(TLIcons.plusImg, SpinnerButtonStyle.GetStyle(), SpinnerButtonOptions))
            {
                Increment();
            }

            if(GUILayout.Button(TLIcons.contractImg, SpinnerButtonStyle.GetStyle(), SpinnerButtonOptions))
            {
                Decrement();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }
    }
}