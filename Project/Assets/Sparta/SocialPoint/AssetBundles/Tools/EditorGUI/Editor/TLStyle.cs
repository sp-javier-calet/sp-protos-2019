using UnityEngine;
using System;
using System.Reflection;
using SocialPoint.Tool.Shared.TLGUI.Utils;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Custom TL class for managing GUIStyle of widgets.
    /// </summary>
    /// TLStyle is a class associated to every widget that describes how that widget should be painted. Mimics the behaviour of GUIStyle.
    /// TLStlye has the benefits of being able to be defined outside the OnGUI loop and that caches it's properties and contents(only rebuilds
    /// when one of it's properties changes) so it's more efficient to call in every paint loop.
	public sealed class TLStyle
	{
        /// <summary>
        /// Custom TL class for managing GUIStyleState of GUIStyle.
        /// </summary>
        /// Mimics the behaviour of GUIStyleState.
        public sealed class TLStyleState
        {
            [NamedMemberAttribute("background")]
            private TLImage   _background;
            public TLImage    background { get { return _background; } set { _background = value;  _parent.ResetCachedStyle(); } }

            [NamedMemberAttribute("textColor")]
            private Color?   _textColor;
            public Color?    textColor { get { return _textColor; } set { _textColor = value;  _parent.ResetCachedStyle(); } }

            TLStyle _parent;

            public TLStyleState(TLStyle parent)
            {
                _parent = parent;
            }
        }

		static private BindingFlags _bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			| BindingFlags.Static;

		private string _baseStyleName;
        /// <summary>
        /// The base style name for using default Unity control styles.
        /// </summary>
		public string baseStyleName { get { return _baseStyleName; } }

		private GUIStyle _cachedStyle;
		private GUIStyle _cachedBaseStyle;

		// GUIStlye reflected properties

		[NamedMemberAttribute("alignment")]
		private TextAnchor? 	_alignment;
		public TextAnchor? 		alignment { get { return _alignment; } set { _alignment = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("active")]
        private TLStyleState 	_active;
        public TLStyleState 	active { get { return _active; } set { _active = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("border")]
		private RectOffset 		_border;
		public RectOffset 		border { get { return _border; } set { _border = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("clipping")]
		private TextClipping? 	_clipping;
		public TextClipping? 	clipping { get { return _clipping; } set { _clipping = value;  ResetCachedStyle(); } }

        [NamedMemberAttribute("contentOffset")]
        private Vector2?        _contentOffset;
        public Vector2?         contentOffset { get { return _contentOffset; } set { _contentOffset = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("fixedHeight")]
		private float?			_fixedHeight;
		public float?			fixedHeight { get { return _fixedHeight; } set { _fixedHeight = value; ResetCachedStyle(); } }

		[NamedMemberAttribute("fixedWidth")]
		private float?			_fixedWidth;
		public float?			fixedWidth { get { return _fixedWidth; } set { _fixedWidth = value; ResetCachedStyle(); } }

		[NamedMemberAttribute("fontSize")]
		private int? 			_fontSize;
		public int? 			fontSize { get { return _fontSize; } set { _fontSize = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("fontStyle")]
		private FontStyle? 		_fontStyle;
		public FontStyle? 		fontStyle { get { return _fontStyle; } set { _fontStyle = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("hover")]
        private TLStyleState 	_hover;
        public TLStyleState 	hover { get { return _hover; } set { _hover = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("imagePosition")]
		private ImagePosition? 	_imagePosition;
		public ImagePosition? 	imagePosition { get { return _imagePosition; } set { _imagePosition = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("margin")]
		private RectOffset 		_margin;
		public RectOffset 		margin { get { return _margin; } set { _margin = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("normal")]
        private TLStyleState 	_normal;
        public TLStyleState 	normal { get { return _normal; } set { _normal = value;  ResetCachedStyle(); } }

        [NamedMemberAttribute("lineHeight")]
        private float?          _lineHeight;
        public float?           lineHeight { get { return _lineHeight; } set { _lineHeight = value; ResetCachedStyle(); } }

		[NamedMemberAttribute("overflow")]
		private RectOffset 		_overflow;
		public RectOffset 		overflow { get { return _overflow; } set { _overflow = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("padding")]
		private RectOffset 		_padding;
		public RectOffset 		padding { get { return _padding; } set { _padding = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("stretchHeight")]
		private bool? 			_stretchHeight;
		public bool? 			stretchHeight { get { return _stretchHeight; } set { _stretchHeight = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("stretchWidth")]
		private bool? 			_stretchWidth;
		public bool? 			stretchWidth { get { return _stretchWidth; } set { _stretchWidth = value;  ResetCachedStyle(); } }

		[NamedMemberAttribute("wordWrap")]
		private bool? 			_wordWrap;
		public bool? 			wordWrap { get { return _wordWrap; } set { _wordWrap = value;  ResetCachedStyle(); } }

		public TLStyle()
		{
			_baseStyleName = "";

            InitStyleStates ();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Tool.Shared.TLGUI.TLStyle"/> class using a default Unity control style.
        /// </summary>
		public TLStyle( string baseStyleName )
		{
			_baseStyleName = baseStyleName;

            InitStyleStates ();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Tool.Shared.TLGUI.TLStyle"/> class copying another style.
        /// </summary>
        /// <param name="other">Other.</param>
		public TLStyle( TLStyle other )
		{
            InitStyleStates ();

			this.CloneFrom (other);
		}

        void InitStyleStates()
        {
            _active = new TLStyleState(this);
            _hover = new TLStyleState(this);
            _normal = new TLStyleState(this);
        }

		void CloneFrom( TLStyle other )
		{
			this._baseStyleName 	= other.baseStyleName;
			this._cachedStyle 		= other._cachedStyle != null ? new GUIStyle (other._cachedStyle) : null;
			this._cachedBaseStyle 	= other._cachedBaseStyle != null ? new GUIStyle (other._cachedBaseStyle) : null;

			this.alignment 		= other.alignment;
			this.active 		= other.active != null ? other.CloneStyleState (other.active) : null;
			this.border 		= other.border != null ? new RectOffset (other.border.left, other.border.right, other.border.top, other.border.bottom) : null;
			this.clipping 		= other.clipping;
            this.contentOffset  = other.contentOffset;
			this.fixedHeight 	= other.fixedHeight;
			this.fixedWidth 	= other.fixedWidth;
			this.fontSize 		= other.fontSize;
			this.fontStyle 		= other.fontStyle;
			this.hover 			= other.hover != null ? other.CloneStyleState (other.hover) : null;
			this.imagePosition 	= other.imagePosition;
			this.margin 		= other.margin != null ? new RectOffset (other.margin.left, other.margin.right, other.margin.top, other.margin.bottom) : null;
			this.normal 		= other.normal != null ? other.CloneStyleState (other.normal) : null;
            this.lineHeight     = other.lineHeight;
			this.overflow 		= other.overflow != null ? new RectOffset (other.overflow.left, other.overflow.right, other.overflow.top, other.overflow.bottom) : null;
			this.padding 		= other.padding != null ? new RectOffset (other.padding.left, other.padding.right, other.padding.top, other.padding.bottom) : null;
			this.stretchHeight 	= other.stretchHeight;
			this.stretchWidth 	= other.stretchWidth;
			this.wordWrap 		= other.wordWrap;
		}

        TLStyleState CloneStyleState( TLStyleState styleState )
		{
            TLStyleState cloned = new TLStyleState (this);
			cloned.background   = styleState.background;
			cloned.textColor    = styleState.textColor;

			return cloned;
		}

        /// <summary>
        /// Use the other TLStyle non-null attributes preserving the current non-null ones.
        /// </summary>
		public void Combine( TLStyle other )
		{
			// Use 'other' non-null attributes(ignore null ones)
			if( other != null ) {
				foreach (FieldInfo finfo in typeof(TLStyle).GetFields (_bindFlags)) {
					object field_value = finfo.GetValue (other);
					if (field_value != null) {
						object[] named_attribute = finfo.GetCustomAttributes (typeof(NamedMemberAttribute), false);
						if( named_attribute.Length > 0 ) {
							NamedMemberAttribute named_att = (NamedMemberAttribute)(named_attribute [0]);
                            PropertyInfo prop = typeof(TLStyle).GetProperty (named_att.Name);
                            //Normal attributes
                            if (prop.PropertyType != typeof(TLStyleState)) {
                                prop.SetValue (this, field_value, null);
                            }
                            //TLStyleState attributes
                            else {
                                object state = prop.GetValue (this, null);

                                foreach (FieldInfo ss_finfo in typeof(TLStyleState).GetFields (_bindFlags)) {
                                    object ss_field_value = ss_finfo.GetValue (field_value);
                                    if (ss_field_value != null) {
                                        object[] ss_named_attribute = ss_finfo.GetCustomAttributes (typeof(NamedMemberAttribute), false);
                                        if( ss_named_attribute.Length > 0 ) {
                                            NamedMemberAttribute ss_named_att = (NamedMemberAttribute)(ss_named_attribute [0]);
                                            PropertyInfo ss_prop = typeof(TLStyleState).GetProperty (ss_named_att.Name);
                                            ss_prop.SetValue (state, ss_field_value, null);
                                        }
                                    }
                                }
                                //prop.SetValue (this, newState, null);
                            }
							
							ResetCachedStyle();
						}
					}
				}
            }
		}

		/// <summary>
        /// Cached style will reset every time a property is changed so it's a good idea
		/// not to change the style once has been defined
        /// </summary>
		public void ResetCachedStyle()
		{
			_cachedStyle = null;
			_cachedBaseStyle = null;
		}

		/// <summary>
		/// Gets the underlaying style. This call must only be made from inside an OnGUI call.
		/// </summary>
		/// <returns>The underlaying, cached if possible, GUIStyle.</returns>
		public GUIStyle GetStyle()
		{
			// If there is no chached style compute one
			if (_cachedStyle == null) {
				if (baseStyleName != "") {
					_cachedBaseStyle = GUI.skin.GetStyle (baseStyleName);

					if (_cachedBaseStyle == null) {
						Debug.LogError (string.Format ("Could not find base \"style\" {0}. New one will be created instead.", baseStyleName));
						_cachedStyle = new GUIStyle ();
					} else {
						_cachedStyle = new GUIStyle (_cachedBaseStyle);
					}
				} else {
					_cachedStyle = new GUIStyle ();
				}

				// Use reflection to get the NamedMemberAttribute on TLStyle
				// Use reflection to set the value on GUIStyle with the equivalent from TLStyle
				// TLStyle uses fields directly while GUIStyle uses properties with the same name, (FieldInfo vs PropertyInfo)
				foreach (FieldInfo finfo in typeof(TLStyle).GetFields (_bindFlags)) {
					object field_value = finfo.GetValue (this);
					if (field_value != null) {
						object[] named_attribute = finfo.GetCustomAttributes (typeof(NamedMemberAttribute), false);
						if( named_attribute.Length > 0 ) {
                            NamedMemberAttribute named_att = (NamedMemberAttribute)(named_attribute [0]);
                            PropertyInfo prop = typeof(GUIStyle).GetProperty (named_att.Name);
                            //Normal attributes
                            if (prop.PropertyType != typeof(GUIStyleState)) {
                                prop.SetValue (_cachedStyle, field_value, null);
                            } 
                            //GuiStyleState attributes
                            else {
                                var state = prop.GetValue (_cachedStyle, null);
                                bool newState = state == null;
                                if (newState)
                                {
                                    state = new GUIStyleState ();
                                }
                                // fill state, it can never be null
                                foreach (FieldInfo ss_finfo in typeof(TLStyleState).GetFields (_bindFlags)) {

                                    object[] ss_named_attribute = ss_finfo.GetCustomAttributes (typeof(NamedMemberAttribute), false);
                                    if( ss_named_attribute.Length > 0 ) {
                                        NamedMemberAttribute ss_named_att = (NamedMemberAttribute)(ss_named_attribute [0]);
                                        object ss_field_value = ss_finfo.GetValue (field_value);
                                        PropertyInfo ss_prop = typeof(GUIStyleState).GetProperty (ss_named_att.Name);

                                        //check if the TLStyleState prop field is null to use the one from the GUIStyleState or overwrite
                                        if (ss_field_value != null)
                                        {
                                            if (ss_prop.PropertyType != typeof(Texture2D)) {
                                                ss_prop.SetValue (state, ss_field_value, null);
                                            }
                                            else {
                                                ss_prop.SetValue (state, (ss_field_value as TLImage).GetTexture(), null);
                                            }
                                        }
                                    }
                                }

                                if(newState)
                                {
                                    prop.SetValue (_cachedStyle, state, null);
                                }
                            }
						}
					}
				}
			}
            // Check for missing references in textures(TLStyleStates)
            else
            {
                if (active != null && active.background != null && (active.background.GetTexture() != null && _cachedStyle.normal.background == null)) {
                    ResetCachedStyle ();
                    return GetStyle();
                }

                if (hover != null && hover.background != null && (hover.background.GetTexture() != null && _cachedStyle.hover.background == null)) {
                    ResetCachedStyle ();
                    return GetStyle();
                }

                if (normal != null && normal.background != null && (normal.background.GetTexture() != null && _cachedStyle.normal.background == null)) {
                    ResetCachedStyle ();
                    return GetStyle();
                }
            }

			return _cachedStyle;
		}

		/// <summary>
		/// Gets the underlaying base control style if any. This call must only be made from inside an OnGUI call.
		/// </summary>
		/// <returns>The underlaying base control, cached if possible, GUIStyle.</returns>
		public GUIStyle GetBaseStyle()
		{
			if (_cachedBaseStyle == null) {
				_cachedBaseStyle = GUI.skin.GetStyle (baseStyleName);
			}

			return _cachedBaseStyle;
		}
	}
}
