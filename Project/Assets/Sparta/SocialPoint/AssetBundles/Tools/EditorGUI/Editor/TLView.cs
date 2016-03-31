using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Base class for all TLView.
    /// </summary>
    /// TLView has all widgets and layouting that defines the look'n feel of the window.
	public abstract class TLView
	{
		List<TLWidget> 			_widgets;
        Action                  _floaterDraws;
		protected TLModel 		_model;
		protected TLController 	_controller;
		TLWindow 				_window;

        /// <summary>
        /// Gets the parent window to which this view is associated.
        /// </summary>
        /// <value>The window.</value>
		public TLWindow window { get { return _window; } }
        public TLModel model { get { return _model; } }
        public TLController controller { get { return _controller; } }

		// Display fields
		Rect? 					_position;
		Vector2?				_minSize;
		Vector2?				_maxSize;
        /// <summary>
        /// Gets or sets the position in the screen.
        /// </summary>
        /// <value>The position.</value>
		public Rect? position { get { return _position; } set { _position = value; } }
        /// <summary>
        /// Gets or sets the minimum size for the associated TLWindow.
        /// </summary>
        /// <value>The minimum size.</value>
		public Vector2? minSize { get { return _minSize; } set { _minSize = value; } }
        /// <summary>
        /// Gets or sets the maximum size for the associated TLWindow.
        /// </summary>
        /// <value>The maximum size.</value>
		public Vector2? maxSize { get { return _maxSize; } set { _maxSize = value; } }

		// Special methods
		bool					_firstRepaint;

		public TLView( TLWindow window, TLModel model )
		{
			_window = window;
			_model = model;
			_widgets = new List<TLWidget>();
			_controller = null;
			_model = model;
			_firstRepaint = true;
		}

        /// <summary>
        /// Sets the controller.
        /// </summary>
        /// <param name="controller">Controller.</param>
		public void SetController( TLController controller )
		{
			_controller = controller;
		}

        /// <summary>
        /// Sets the window.
        /// </summary>
        /// <param name="window">Window.</param>
		public void SetWindow( TLWindow window )
		{
			_window = window;
		}

		public void Draw()
		{
			if (_firstRepaint)
				OnFirstRepaintBegin ();

			for (int i = 0; i < _widgets.Count; i++) {
				if(_widgets [i].IsVisible)
					_widgets [i].Draw ();
			}

            if (_floaterDraws != null)
                _floaterDraws();

			if (_firstRepaint) {
				OnFirstRepaintEnd ();
				_firstRepaint = false;
			}
		}

		public void Update(double elapsed)
		{
			for (int i = 0; i < _widgets.Count; i++) {
				if(_widgets [i].IsVisible)
					_widgets [i].Update (elapsed);
			}

			if ( _controller == null ) return;

			_controller.Update(elapsed);
		}

        /// <summary>
        /// Adds the widget to this window for control and display.
        /// </summary>
        /// <returns>The widget.</returns>
        /// <param name="widget">Widget.</param>
		public TLWidget AddWidget( TLWidget widget )
		{
			_widgets.Add( widget );
			return widget;
		}

        /// <summary>
        /// (Internal Use Only)Adds the draw method of the TLFloater widget.
        /// </summary>
        /// <param name="floaterDraw">TLFloater draw method.</param>
        public void AddFloaterDraw(Action floaterDraw)
        {
            _floaterDraws += floaterDraw;
        }

        /// <summary>
        /// Method called whenever the view is loaded in a TLWindow. Should be overrided on subclasses.
        /// </summary>
		public virtual void OnLoad()
		{
			Refresh();
			if ( _controller != null ) {
				_controller.OnLoad();
			}
		}

		public virtual void OnDestroy()
		{
			for (int i = 0; i < _widgets.Count; i++) {
				_widgets [i].OnDestroy ();
			}

            if(_controller != null)
            {
                _controller.OnUnload();
            }
		}

		public virtual void Refresh() {}

		public virtual void OnFirstRepaintBegin() {}
		public virtual void OnFirstRepaintEnd() {}

		public void Load()
		{
			window.LoadView( this );
		}
	}
}
