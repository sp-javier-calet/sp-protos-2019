using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Tree Node Check.
    /// </summary>
    /// This Widget is undocumented and it's behaviour has not been tested properly.
	public class TLWTreeNodeCheck : TLWTreeNode
	{
		private TLWCheck _check;
		public TLWCheck check { get { return _check; } }

		public TLWTreeNodeCheck( TLView view, string name, string text, TLWTreeNode[] items ): base ( view, name, null, "", items )
		{
			Init( view, name, text );
		}
		
		public TLWTreeNodeCheck( TLView view, string name, string text ): base ( view, name, null, "" )
		{
			Init( view, name, text );
		}
		
		private void Init( TLView view, string name, string text )
		{
			_check = new TLWCheck( view, name, text );
			_check.onCheckEvent.Connect( OnCheck );
		}

		protected override void DrawIcon()
		{
			_check.Draw();
		}
		
		protected override void DrawChildren(  int depth, int indentation )
		{
			base.DrawChildren( depth, indentation );
		}

		public void OnCheck(bool value)
		{
			SetCheckOnChildren( value, this );
		}

		private void SetCheckOnChildren( bool value, TLWTreeNode root )
		{
			for ( int i = 0; i < root.items.Length; i++ ) {
				TLWTreeNode node = root.items[i];
				if ( node is TLWTreeNodeCheck ) {
					( node as TLWTreeNodeCheck ).check.SetCheck( value );
				}
				SetCheckOnChildren( value, node );
			}
		}
	}
}

