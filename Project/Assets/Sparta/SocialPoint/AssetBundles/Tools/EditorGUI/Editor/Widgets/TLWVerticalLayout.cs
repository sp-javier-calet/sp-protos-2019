using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A widget for laying other widgets in a column, vertically.
    /// </summary>
	public class TLWVeticalLayout : TLWidget 
	{	
		private List<TLWidget> _widgets;

        public float spacing { get; set; }
		
		public TLWVeticalLayout( TLView view, string name ): base ( view, name, TLLayoutOptions.vertical )
		{
			_widgets = new List<TLWidget>();
            spacing = 0;
		}

        public TLWVeticalLayout( TLView view, string name, TLStyle style, GUILayoutOption[] options ): base ( view, name, style, options )
        {
            _widgets = new List<TLWidget>();
            spacing = 0;
        }

		public TLWVeticalLayout( TLView view, string name, GUILayoutOption[] options ): base ( view, name, options )
		{
			_widgets = new List<TLWidget>();
            spacing = 0;
		}
		
		public override void Perform()
		{
			if (_widgets.Count > 0) {
				GUILayout.BeginVertical ( GetStyle(), Options );
				for ( int i = 0; i < _widgets.Count; i++ )
                {
					if(_widgets [i].IsVisible)
						_widgets[i].Draw();
                    if(spacing > 0)
                        GUILayout.Space(spacing);
                }
				GUILayout.EndVertical ();
			}
		}

		public override void Update(double elapsed)
		{
			for (int i = 0; i < _widgets.Count; i++) {
				if(_widgets [i].IsVisible)
					_widgets [i].Update (elapsed);
			}
		}
		
		public TLWidget AddWidget( TLWidget widget )
		{
			_widgets.Add (widget);
			return widget;
		}
	}
}
