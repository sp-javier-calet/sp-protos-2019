using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Button Widget.
    /// </summary>
    /// Can send click events.
	public class TLWButton: TLWidget
	{
        private TLEvent _onClickEvent;
		private string _text;
		private GUIContent _guiTex;
		private GUILayoutOption[] _texOptions;

		private enum Type { TEXT, IMAGE }
		private Type _type;

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text on the button.</value>
		public string text { get { return _text; } }
        /// <summary>
        /// Gets click event to connect to.
        /// </summary>
        /// <value>The click event to connect to.</value>
		public TLEvent onClickEvent { get { return _onClickEvent; } }

		public TLWButton( TLView view, string name, string text ): base ( view, name, TLLayoutOptions.noexpand )
		{
			InitText( text );
			Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
		}

		public TLWButton( TLView view, string name, string text, TLStyle style ): base ( view, name, style, TLLayoutOptions.noexpand )
		{
			InitText( text );
		}

		public TLWButton( TLView view, string name, string text, GUILayoutOption[] options ): base ( view, name, options )
		{
			InitText( text );
			Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
		}

        public TLWButton( TLView view, string name, string text, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
        {
            InitText( text );
        }

		public TLWButton( TLView view, string name, Texture2D tex ): base ( view, name )
		{
			float dim = Mathf.Max( tex.width, tex.height );
			InitTexture( tex, dim, dim );
			Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
		}

        public TLWButton( TLView view, string name, Texture2D tex, float width, float height ): base ( view, name )
        {
            InitTexture( tex, width, height );
            Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
        }
		
        public TLWButton( TLView view, string name, Texture2D tex, float width, float height, TLStyle style ): base ( view, name, style )
		{
			InitTexture( tex, width, height );
		}

		public TLWButton( TLView view, string name, Texture2D tex, GUILayoutOption[] options ): base ( view, name, options )
		{
			float dim = Mathf.Max( tex.width, tex.height );
			InitTexture( tex, dim, dim );
			Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
		}

		public TLWButton( TLView view, string name, Texture2D tex, float width, float height, GUILayoutOption[] options ): base ( view, name, options )
		{
			InitTexture( tex, width, height );
			Style = new TLStyle ("Button");
            Style.margin = new RectOffset();
		}

        public TLWButton( TLView view, string name, Texture2D tex, float width, float height, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
        {
            InitTexture( tex, width, height );
        }

		private void InitText( string text )
		{
			_onClickEvent = new TLEvent( "OnClicked" );
			_text = text;
			_type = Type.TEXT;
		}

		private void InitTexture( Texture2D tex, float width, float height )
		{
			_onClickEvent = new TLEvent( "OnClicked" );
			_guiTex = new GUIContent( tex );
			_texOptions = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };
			_type = Type.IMAGE;
		}

        public void SetImage(Texture2D tex, float width, float height)
        {
            _guiTex = new GUIContent( tex );
            _type = Type.IMAGE;
        }

		public override void Perform()
		{
			if ( _type == Type.TEXT ) {
				if ( GUILayout.Button ( _text, GetStyle(), Options ) ) {
					View.window.eventManager.AddEvent( onClickEvent );
				}
			}
			else {
				if ( GUILayout.Button ( _guiTex, GetStyle(), _texOptions ) ) {
					View.window.eventManager.AddEvent( onClickEvent );
				}
			}
		}
	}
}
