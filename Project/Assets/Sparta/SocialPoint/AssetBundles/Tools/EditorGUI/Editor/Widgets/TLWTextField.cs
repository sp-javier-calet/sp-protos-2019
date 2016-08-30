using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// An Edit box for text.
    /// </summary>
	public sealed class TLWTextField : TLWidget 
	{
		Rect _lastRect;
        int _maxLength = 0;
        /// <summary>
        /// Gets or sets the max length of the text.
        /// </summary>
        /// <value>The max length of the text.</value>
        public int maxLength { get { return _maxLength; } set { AssignMaxLength(value); } }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SocialPoint.Tool.Shared.TLGUI.TLWTextField"/> allows
        /// line breaks.
        /// </summary>
        /// <value><c>true</c> if allow line breaks; otherwise, <c>false</c>.</value>
        public bool allowLineBreaks { get; set; }

        bool _readOnly;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SocialPoint.Tool.Shared.TLGUI.TLWTextField"/> is read only.
        /// </summary>
        /// In read-only mode the box will always be a single line, and if the text cannot be allocated it(the displayed text only) will be truncated from its origin to fit the box.
        /// <value><c>true</c> if is read only; otherwise, <c>false</c>.</value>
        public bool readOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                Style.wordWrap = value;
                _readOnly = value;
            }
        }
		
        string _text;
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
		public string text 
        { 
            get 
            { 
                return _text;
            }
            set
            {
                _textContent.text = value;
                _text = value;
                _trimmed = false;
            }
        }

        GUIContent _textContent;
        bool _trimmed;

        public TLWTextField( TLView view, string name ): base ( view, name )
		{
			_text = "";
            _lastRect = new Rect();
            _textContent = new GUIContent("");
			Style = new TLStyle ("TextField");
            Style.margin = new RectOffset();
            _trimmed = false;
            allowLineBreaks = false;
		}

        public TLWTextField( TLView view, string name, string text ): base ( view, name )
        {
            _text = text;
            _textContent = new GUIContent(text);
            _lastRect = new Rect();
            Style = new TLStyle ("TextField");
            Style.margin = new RectOffset();
            _trimmed = false;
            allowLineBreaks = false;
        }
		
		public TLWTextField( TLView view, string name, string text, TLStyle style ): base ( view, name, style )
		{
            _text = text;
            _textContent = new GUIContent(text);
            _lastRect = new Rect();
            _trimmed = false;
            allowLineBreaks = false;
		}
		
		public TLWTextField( TLView view, string name, string text, GUILayoutOption[] options ): base ( view, name, options )
		{
            _text = text;
            _textContent = new GUIContent(text);
            _lastRect = new Rect();
			Style = new TLStyle ("TextField");
            Style.margin = new RectOffset();
            _trimmed = false;
            allowLineBreaks = false;
		}
		
		public TLWTextField( TLView view, string name, string text, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
		{
            _text = text;
            _textContent = new GUIContent(text);
            _lastRect = new Rect();
            _trimmed = false;
            allowLineBreaks = false;
		}
		
		public override void Perform()
		{
			GUI.SetNextControlName (Name);

            if (!readOnly) {
                text = _maxLength > 0 ? GUILayout.TextField( text, _maxLength, GetStyle(), Options ) : GUILayout.TextField( text, GetStyle(), Options );
                if (!allowLineBreaks) {
                    text = text.Replace("\n",string.Empty);
                }
            }
            else {
                int keyboardControl_bkp = GUIUtility.keyboardControl;
                GUIUtility.keyboardControl = 0;
                GUILayout.TextField( _textContent.text, GetStyle(), Options );
                GUIUtility.keyboardControl = keyboardControl_bkp;

                if (Event.current.type == EventType.Repaint)
                    _lastRect = GUILayoutUtility.GetLastRect ();

                // CalcSize does not take into account word wrapping, which is exactly what we need
                var objectiveHeight = Style.GetStyle().CalcSize(_textContent).y;
                var currentHeight = Style.GetStyle().CalcHeight(_textContent, _lastRect.width);

                if (objectiveHeight < currentHeight)
                    wrapToFit(_lastRect, objectiveHeight, currentHeight);

                if (_lastRect.Contains(Event.current.mousePosition) && _trimmed) {
                    GUI.tooltip = _text;
                }
            }
		}

        void wrapToFit(Rect fitRect, float objectiveHeight, float currentHeight)
        {
            while (objectiveHeight < currentHeight && _text.Length > 3) {
                _trimmed = true;
                _textContent.text = _textContent.text.Substring(1);

                objectiveHeight = Style.GetStyle().CalcSize(_textContent).y;
                currentHeight = Style.GetStyle().CalcHeight(_textContent, _lastRect.width);
            }

            if (_trimmed && _textContent.text.Length > 3)
                _textContent.text = "..." + _textContent.text.Substring(3);
        }

        void AssignMaxLength(int maxLength)
        {
            if (maxLength < 0)
            {
                _maxLength = 0;
                Debug.LogWarning ("TLWTextField: maxLength cannot be less than 0.");
            }
            else
                _maxLength = maxLength;
        }
	}
}

