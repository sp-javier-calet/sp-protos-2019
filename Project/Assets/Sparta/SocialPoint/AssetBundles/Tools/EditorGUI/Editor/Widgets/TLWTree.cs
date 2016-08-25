using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Tree Widget.
    /// </summary>
    /// This Widget is undocumented and it's behaviour has not been tested properly.
	public sealed class TLWTree : TLWidget
	{
		private TLWTreeNode[] _items;
		private int _indentation;

		private const int DEFAULT_INDENTATION = 20;

		public TLWTreeNode[] items { get { return _items; } }

		public TLWTree( TLView view, string name, TLWTreeNode[] items ): base ( view, name )
		{
			_items = items;
			_indentation = DEFAULT_INDENTATION;
		}

		public TLWTree( TLView view, string name, int indentation, TLWTreeNode[] items ): base ( view, name )
		{
			_items = items;
			_indentation = indentation;
		}

		public override void Perform ()
		{
			for ( int i = 0; i < _items.Length; i++ ) {
				_items[i].DrawInDepth( 0, _indentation );
			}
		}

		public void SetItems( TLWTreeNode[] items )
		{
			_items = items;
		}
	}
}
