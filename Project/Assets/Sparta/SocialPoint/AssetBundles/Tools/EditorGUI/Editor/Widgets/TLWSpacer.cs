using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A widget for consuming horizontal or vertical spacing.
    /// </summary>
    /// This (invisible)widget will take horizontal or vertical space when used in a TLWHorizontalLayout or TLVerticalLayout respectively.
    /// Its expandible property tells if the widget takes as many space as possible or a fixed amount.
	public sealed class TLWSpacer : TLWidget 
	{
		int _space;
		bool _expandible;

		public TLWSpacer( TLView view, string name, int space ): base ( view, name )
		{
			_space = space;
		}

		public TLWSpacer( TLView view, string name, bool expandible ): base ( view, name )
		{
			_expandible = expandible;
		}
		
		public override void Perform()
		{
			if (_expandible)
				GUILayout.FlexibleSpace ();
			else
				GUILayout.Space (_space);
		}
	}
}
