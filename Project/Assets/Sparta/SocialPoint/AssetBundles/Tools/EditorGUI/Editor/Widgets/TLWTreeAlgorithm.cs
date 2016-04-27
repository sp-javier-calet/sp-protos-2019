using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Tree Algorithm.
    /// </summary>
    /// This class is undocumented and it's behaviour has not been tested properly.
	public static class TLWTreeAlgorithm
	{
		public static TLWCheck[] GetCheckSelection( TLWTree tree, bool isSelected = true )
		{
			List<TLWCheck> checkList = new List<TLWCheck>();

			for ( int i = 0; i < tree.items.Length; i++ ) {
				checkList.AddRange( GetCheckSelection( tree.items[i], isSelected ) );
			}

			return checkList.ToArray();
		}

		public static TLWCheck[] GetCheckSelection( TLWTreeNode root, bool isSelected = true )
		{
			List<TLWCheck> checkList = new List<TLWCheck>();

			if ( root is TLWTreeNodeCheck ) {
				TLWTreeNodeCheck checkNode = root as TLWTreeNodeCheck;
				if ( checkNode.check.isChecked == isSelected ) {
					checkList.Add( checkNode.check );
				}
			}

			for ( int i = 0; i < root.items.Length; i++ ) {
				checkList.AddRange( GetCheckSelection( root.items[i], isSelected ) );
			}

			return checkList.ToArray();
		}
	}
}
