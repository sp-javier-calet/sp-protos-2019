using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// An Edit box for numbers only.
    /// </summary>
    /// Numbers can be integer or decimal.
    public class TLWNumberField : TLWidget 
    {
        private string _number;
        private int _maxLength;

        /// <summary>
        /// Gets or sets the number as a string.
        /// </summary>
        /// <value>The number as a string.</value>
        public string number { get { return _number; } set { _number = value; } }
        
        public TLWNumberField( TLView view, string name ): base ( view, name )
        {
            _number = "0";
            AssignMaxLength(11);
            Style = new TLStyle ("TextField");
        }
        
        public TLWNumberField( TLView view, string name, string number, int maxLength, TLStyle style ): base ( view, name, style )
        {
            _number = number;
            AssignMaxLength(maxLength);
        }
        
        public TLWNumberField( TLView view, string name, string number, int maxLength, GUILayoutOption[] options ): base ( view, name, options )
        {
            _number = number;
            AssignMaxLength(maxLength);
            Style = new TLStyle ("TextField");
        }
        
        public TLWNumberField( TLView view, string name, string number, int maxLength, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
        {
            _number = number;
            AssignMaxLength(maxLength);
        }
        
        public override void Perform()
        {
            GUI.SetNextControlName (Name);
            var input = GUILayout.TextField( _number, _maxLength, GetStyle(), Options );

            //swallow line breaks
            input = input.Replace("\n",string.Empty);

            // int field
            int outVal;
            if (int.TryParse(input, out outVal)) {
                _number = input;
            }
        }

        void AssignMaxLength(int maxLength)
        {
            if (maxLength <= 0 || maxLength > 32)
            {
                _maxLength = 11;
                Debug.LogWarning ("TLWNumberField: maxLength cannot be less than 1 or more than 32. Defaulting to 11.");
            }
            else
                _maxLength = maxLength;
        }
    }
}

