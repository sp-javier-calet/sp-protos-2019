using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Tree Node.
    /// </summary>
    /// This Widget is undocumented and it's behaviour has not been tested properly.
	public class TLWTreeNode : TLWidget
	{
		private TLWTreeNode[] _items;
		private GUIContent _guiTex;
		private string _text;

		private const int DEFAULT_INDENTATION = 20;

		public TLWTreeNode[] items { get { return _items; } }

		public TLWTreeNode( TLView view, string name, Texture2D tex, string text, TLWTreeNode[] items ): base ( view, name )
		{
			Init ( tex, text, items );
		}

		public TLWTreeNode( TLView view, string name, Texture2D tex, string text ): base ( view, name )
		{
			Init ( tex, text, new TLWTreeNode[] {} );
		}

		private void Init( Texture2D tex, string text, TLWTreeNode[] items )
		{
			_guiTex = new GUIContent( tex );
			_text = text;
			_items = items;
		}

		public void SetItems( TLWTreeNode[] items )
		{
			_items = items;
		}

		public override void Perform ()
		{
			DrawInDepth( 0, DEFAULT_INDENTATION );
		}

		public void DrawInDepth( int depth, int indentation )
		{
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label( " ", GUILayout.Width( depth*indentation ) );
			DrawIcon();
			GUILayout.Label( _text, TLLayoutOptions.noexpand );
			EditorGUILayout.EndHorizontal ();

			DrawChildren( depth, indentation );
		}

		protected virtual void DrawIcon()
		{
			GUILayout.Label( _guiTex, TLLayoutOptions.noexpand );
		}

		protected virtual void DrawChildren(  int depth, int indentation  )
		{
			for ( int i = 0; i < _items.Length; i++ ) {
				_items[i].DrawInDepth( depth+1, indentation );
			}
		}
	}
}
