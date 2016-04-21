using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// An agroupation of Widgets.
    /// </summary>
    /// The TLWGroup allows to group widgets in the view for managing all of them from a single parent instance.
    /// A TLWGroup can manage all the Widgets common properties from a single instance.
    /// Widgets added to a TLWGroup should not be individually added to TLWLayout nor TLView, the TLWGroup must be added instead.
    /// Recursive nested TLWGroups or TLWLayouts will incurr in an infinite recursivity.
    public class TLWGroup : TLWidget 
    {
        private List<TLWidget> _widgets;

        public TLWGroup( TLView view, string name ): base ( view, name )
        {
            _widgets = new List<TLWidget>();
        }
        
        public override void Perform()
        {
            for ( int i = 0; i < _widgets.Count; i++ )
                if(_widgets [i].IsVisible)
                    _widgets[i].Draw();
        }
        
        public override void Update(double elapsed)
        {
            for (int i = 0; i < _widgets.Count; i++) {
                if(_widgets [i].IsVisible)
                    _widgets [i].Update (elapsed);
            }
        }

        /// <summary>
        /// Adds the widget to the group.
        /// </summary>
        /// <returns>The widget added.</returns>
        /// <param name="widget">Widget.</param>
        public TLWidget AddWidget( TLWidget widget )
        {
            //TODO: Control that there aren't recursive nested groups

            _widgets.Add (widget);
            return widget;
        }

        /// <summary>
        /// Sets the visibility for all the widgets in the group.
        /// </summary>
        /// <param name="value">Value of the visibility.</param>
        public override void SetVisible( bool value )
        {
            for (int i = 0; i < _widgets.Count; i++) {
                _widgets [i].SetVisible (value);
            }
            base.SetVisible (value);
        }

        /// <summary>
        /// Sets the disabled state for all the widgets in the group.
        /// </summary>
        /// <param name="value">Value of the disabled state.</param>
        public override void SetDisabled( bool value )
        {
            for (int i = 0; i < _widgets.Count; i++) {
                _widgets [i].SetDisabled (value);
            }
            base.SetDisabled (value);
        }
    }
}