using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A widget for laying widgets in a confined, predefined rectangle.
    /// </summary>
    /// As opposed to TLWHorizontalLayout and TLWVerticalLayout, there cannot be nested TLWLayouts.
	public sealed class TLWLayout : TLWidget 
	{	
		private List<TLWidget> _widgets;
		private Rect _rect;

		public TLWLayout( TLView view, string name, Rect rect ): base ( view, name )
		{
			_widgets = new List<TLWidget>();
			_rect = rect;
		}
		
		public override void Perform()
		{
			GUILayout.BeginArea (_rect, GetStyle());
			for ( int i = 0; i < _widgets.Count; i++ )
				if(_widgets [i].IsVisible)
					_widgets[i].Draw();
			GUILayout.EndArea ();
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
        /// <returns>The widget added.</returns>
        /// <param name="widget">Widget.</param>
		public TLWidget AddWidget( TLWidget widget )
		{
			if (widget.GetType () != typeof(TLWLayout)) {
				_widgets.Add (widget);
				return widget;
			} else {
				throw new Exception("Cannot nest TLWLayout classes!");
			}
		}
	}
}
