using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A widget for laying other widgets in a scrollable view.
    /// </summary>
	public sealed class TLWScroll : TLWidget 
	{
		private List<TLWidget> _widgets;
		private Vector2 _scrollPosition;

		public TLWScroll( TLView view, string name ): base ( view, name, TLLayoutOptions.expandall )
		{
			_widgets = new List<TLWidget>();
		}
		
		public override void Perform()
		{
			if (_widgets.Count > 0) {
				_scrollPosition = GUILayout.BeginScrollView (_scrollPosition, Options);
				for (int i = 0; i < _widgets.Count; i++)
					if(_widgets [i].IsVisible)
						_widgets [i].Draw ();
				GUILayout.EndScrollView ();
			}
		}

		public override void Update(double elapsed)
		{
			for (int i = 0; i < _widgets.Count; i++) {
				if(_widgets [i].IsVisible)
					_widgets [i].Update (elapsed);
			}
		}

        /// <summary>
        /// Adds the widget to this layout.
        /// </summary>
        /// <param name="widget">Widget.</param>
		public void AddWidget( TLWidget widget )
		{
			_widgets.Add( widget );
		}
	}
}
