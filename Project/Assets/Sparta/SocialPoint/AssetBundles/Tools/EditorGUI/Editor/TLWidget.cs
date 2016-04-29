using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Base class for all widgets that are added to TLView.
    /// </summary>
    /// TLWidget is tha class that represents a control or a group of controls that have display properties and logic.
    /// Every subclass of TLWidget will have its unique Perform method that describes how it's painted and a possible Update method
    /// to describe more complex logic.
    /// Also every TLWidget should have at least one TLStyle that defines its looks.
	public class TLWidget
	{
		private string _name;
		private bool _isVisible;
		private bool _isDisabled;
		private bool _isFocused;
		private TLView _view;
		private TLStyle _style;
		private GUILayoutOption[] _options;

        /// <summary>
        /// Gets the parent view that this control is appended to.
        /// </summary>
        /// <value>The view.</value>
		public TLView View { get { return _view; } }
        /// <summary>
        /// Gets the name of this control. It should be unique inside the view.
        /// </summary>
        /// <value>The name.</value>
		public string Name { get { return _name; } }
        /// <summary>
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value><c>true</c> if this instance is visible; otherwise, <c>false</c>.</value>
		public bool IsVisible { get { return _isVisible; } }
        /// <summary>
        /// Gets or sets the style for this control.
        /// </summary>
        /// <value>The style.</value>
		public TLStyle Style { get { return _style; } protected set { _style = value; } }
        /// <summary>
        /// Gets or sets the layouting options.
        /// </summary>
        /// Can determine a fixed size or a flexible size inside a layout.
        /// <value>The options.</value>
		public GUILayoutOption[] Options { get { return _options; } protected set { _options = value; } }
        /// <summary>
        /// Gets a value indicating whether this instance is focused.
        /// </summary>
        /// <value><c>true</c> if this instance is focused; otherwise, <c>false</c>.</value>
		public bool IsFocused { get { return _isFocused; } }
        /// <summary>
        /// Gets a value indicating whether this instance is disabled.
        /// </summary>
        /// <value><c>true</c> if this instance is disabled; otherwise, <c>false</c>.</value>
		public bool IsDisabled { get { return _isDisabled; } }

		public TLWidget( TLView view, string name )
		{
			_view = view;
			_name = name;
			_isVisible = true;
			_style = null;
			_options = TLLayoutOptions.basic;
		}

		public TLWidget( TLView view, string name, TLStyle style )
		{
			_view = view;
			_name = name;
			_isVisible = true;
			_style = style;
			_options = TLLayoutOptions.basic;
		}

		public TLWidget( TLView view, string name, GUILayoutOption[] options )
		{
			_view = view;
			_name = name;
			_isVisible = true;
			_style = null;
			_options = options;
		}

		public TLWidget( TLView view, string name, TLStyle style, GUILayoutOption[] options )
		{
			_view = view;
			_name = name;
			_isVisible = true;
			_style = style;
			_options = options;
		}

		/// <summary>
        /// Gets the underlying style for the widget TLStyle.
        /// </summary>
        /// Must be called inside an OnGUI loop(Perform).
        /// <returns>The style.</returns>
		public GUIStyle GetStyle()
		{
			if (Style != null)
				return Style.GetStyle ();
			else
				return GUIStyle.none;
		}

		public void Draw()
		{
			// This should be overridden in the Widget's custom Perform call if multiple controls are drawn
			GUI.SetNextControlName (Name);

			bool bkpEnabled = GUI.enabled;
			if (GUI.enabled && IsDisabled)
				GUI.enabled = false;

			Perform();

			GUI.enabled = bkpEnabled;

			_isFocused = GUI.GetNameOfFocusedControl () == Name;
		}

		public virtual void Update(double elapsed) {}

		public virtual void Perform() {}

		public virtual void OnDestroy() {}

        public virtual void SetVisible( bool value )
		{
			_isVisible = value;
		}

        public virtual void SetDisabled( bool value )
		{
			_isDisabled = value;
		}
	}
}
