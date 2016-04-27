using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A base widget class for TLWidget that needs to be (at least part of it)drawn on top of others.
    /// </summary>
    /// TLFloater is a special base class for TLWidgets that has (part of)its drawing method performed OVER other widgets that are drawn later.
    /// In the Unity Editor drawing pipeline, the GUI elements that are drawn later always get painted on top of the previous. This class bypasses, with
    /// some restrictions, that functionality.
    /// The known issues are that input events(mouse) are processed by the widgets that are drawn first, so if there's a widget under the TLFloater
    /// this(the underlaying) widget will get the input. If the underlaying widget is disabled, the TLFloater will recevie the input. So some manually setup must be
    /// made in order for this to work properly.
    public class TLFloater : TLWidget
    {
        protected Rect paintArea;

        public TLFloater( TLView view, string name ) : base(view, name)
        {
            paintArea = new Rect();
            LinkView();
        }
        
        public TLFloater( TLView view, string name, TLStyle style ) : base(view, name, style)
        {
            paintArea = new Rect();
            LinkView();
        }
        
        public TLFloater( TLView view, string name, GUILayoutOption[] options ) : base(view, name, options)
        {
            paintArea = new Rect();
            LinkView();
        }
        
        public TLFloater( TLView view, string name, TLStyle style, GUILayoutOption[] options ) : base(view, name, style, options)
        {
            paintArea = new Rect();
            LinkView();
        }

        void LinkView()
        {
            View.AddFloaterDraw(PerformFloater);
        }

        /// <summary>
        /// Drawing method that draws to pieces needed to be on top of other widgets.
        /// </summary>
        /// This methoud should be overriden by subclassing widgets.
        protected virtual void PerformFloater() {}
    }
}