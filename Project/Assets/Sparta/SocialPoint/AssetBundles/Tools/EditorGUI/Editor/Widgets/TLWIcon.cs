using UnityEditor;
using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI.Utils;
using System;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// An Image widget with optional text.
    /// </summary>
    /// The TLWIcon widget can be used to place Images as widgets, benefiting from the TLWidget styling and layouting capabilities.
	public sealed class TLWIcon: TLWidget
	{
		private GUIContent _guiTex;
		private string _text;
		private TLImage _image;

        /// <summary>
        /// Gets the text(if any) fot the icon.
        /// </summary>
        /// <value>The text.</value>
		public string text { get { return _text; } }

		// TLImage constructors

		public TLWIcon( TLView view, string name, TLImage image ): base ( view, name, TLLayoutOptions.noexpand )
		{
			_image = image;
			_text = "";
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, TLImage image, TLStyle style ): base ( view, name, style, TLLayoutOptions.noexpand )
		{
			_image = image;
			_text = "";
		}

		public TLWIcon( TLView view, string name, TLImage image, GUILayoutOption[] options ): base ( view, name, options )
		{
			_image = image;
			_text = "";
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, TLImage image, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
		{
			_image = image;
			_text = "";
		}

		public TLWIcon( TLView view, string name, TLImage image, string text ): base ( view, name, TLLayoutOptions.noexpand )
		{
			_image = image;
			_text = text;
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, TLImage image, string text, TLStyle style ): base ( view, name, style, TLLayoutOptions.noexpand )
		{
			_image = image;
			_text = text;
		}

		public TLWIcon( TLView view, string name, TLImage image, string text, GUILayoutOption[] options ): base ( view, name, options )
		{
			_image = image;
			_text = text;
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, TLImage image, string text, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
		{
			_image = image;
			_text = text;
		}

		// Texrure2D constructors

		public TLWIcon( TLView view, string name, Texture2D tex ): base ( view, name, TLLayoutOptions.noexpand )
		{
			_guiTex = new GUIContent( tex );
			_text = "";
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, Texture2D tex, GUILayoutOption[] options ): base ( view, name, options )
		{
			_guiTex = new GUIContent( tex );
			_text = "";
			Style = new TLStyle ("Label");
		}

		public TLWIcon( TLView view, string name, Texture2D tex, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
		{
			_guiTex = new GUIContent( tex );
			_text = "";
		}

		public TLWIcon( TLView view, string name, Texture2D tex, string text ): base ( view, name, TLLayoutOptions.noexpand )
		{
			_guiTex = new GUIContent( tex );
			_text = text;
			Style = new TLStyle ("Label");
		}

		public override void Perform ()
		{
			Texture2D iconTexture = _image != null ? _image.GetTexture () : (Texture2D)_guiTex.image;
			if (_text != "") {
				GUILayout.Label( new GUIContent(_text, iconTexture), GetStyle(), Options );
			} else {
				GUILayout.Label( iconTexture, GetStyle(), Options );
			}
		}

		public override void Update (double elapsed)
		{
			bool repaintNeeded = false;

			if(_image != null && _image.Type == TLImageType.TLAnimatedImage) {
				TLAnimatedImage animImage = (TLAnimatedImage)_image;
				repaintNeeded |= animImage.Update(elapsed);
			}
			if (repaintNeeded){
				View.window.Repaint ();
			}
		}

        /// <summary>
        /// DEPRECATED. Sets the texture.
        /// </summary>
        /// Use SetImage method instead.
        /// <param name="tex">Tex.</param>
		public void SetTexture( Texture2D tex )
		{
			_guiTex = new GUIContent( tex );
		}

        /// <summary>
        /// Sets the TLImage for the widget.
        /// </summary>
        /// <param name="image">Image.</param>
		public void SetImage( TLImage image )
		{
			_image = image;
		}
	}
}
