using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Tree Node Expand.
    /// </summary>
    /// This Widget is undocumented and it's behaviour has not been tested properly.
	public class TLWTreeNodeExpand : TLWTreeNode
	{
		private bool _isEnabled;
		private TLWButton _buttonEnable;
		private TLWButton _buttonDisable;

		private TLWButton button { get { return _isEnabled ? _buttonDisable: _buttonEnable; } }

		public TLWTreeNodeExpand( TLView view, string name, Texture2D enableTex, Texture2D disableTex, string text, TLWTreeNode[] items ): base ( view, name, enableTex, text, items )
		{
			Init( view, name, enableTex, disableTex );
		}
		
		public TLWTreeNodeExpand( TLView view, string name, Texture2D enableTex, Texture2D disableTex, string text ): base ( view, name, enableTex, text )
		{
			Init( view, name, enableTex, disableTex );
		}

		private void Init( TLView view, string name, Texture2D enableTex, Texture2D disableTex )
		{
			_isEnabled = true;
			_buttonEnable = new TLWButton( view, name+"_buttonEnable", enableTex, 20.0f, 20.0f );
			_buttonDisable = new TLWButton( view, name+"_buttonDisable", disableTex, 20.0f, 20.0f );
			_buttonEnable.onClickEvent.Connect( OnEnable );
			_buttonDisable.onClickEvent.Connect( OnDisable );
		}

		public void OnEnable()
		{
			_isEnabled = true;
		}

		public void OnDisable()
		{
			_isEnabled = false;
		}

		protected override void DrawIcon()
		{
			button.Draw();
		}

		protected override void DrawChildren(  int depth, int indentation )
		{
			if ( !_isEnabled ) return;

			base.DrawChildren( depth, indentation );
		}
	}
}
