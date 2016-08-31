using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Label Widget.
    /// </summary>
    /// A Label for displaying text.
	public sealed class TLWLabel : TLWidget 
	{
		private string _text;

        /// <summary>
        /// Gets or sets the text for the label.
        /// </summary>
        /// <value>The text.</value>
		public string text { get { return _text; } set { _text = value; } }
		
		public TLWLabel( TLView view, string name, string text ): base ( view, name )
		{
			_text = text;
			Style = new TLStyle ("Label");
            Style.margin = new RectOffset();
		}

		public TLWLabel( TLView view, string name, string text, TLStyle style ): base ( view, name, style )
		{
			_text = text;
		}

		public TLWLabel( TLView view, string name, string text, GUILayoutOption[] options ): base ( view, name, options )
		{
			_text = text;
			Style = new TLStyle ("Label");
            Style.margin = new RectOffset();
		}

		public TLWLabel( TLView view, string name, string text, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
		{
			_text = text;
		}
		
		public override void Perform()
		{
			GUILayout.Label( _text, GetStyle(), Options );
		}
	}
}
