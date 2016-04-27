using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// The base class for all TLView controllers.
    /// </summary>
    /// TLController holds all the logic and actions performed in a view as well as the communication with the TLModel class.
	public class TLController
	{
		protected TLView _view;
		protected TLModel _model;
        /// <summary>
        /// Gets or sets the on load actions.
        /// </summary>
        /// By adding Action to this property, when the TLContoller gets loaded, all the Action are performed.
		public Action OnLoadActions { get; set; }

        /// <summary>
        /// Gets the view associated to this controller.
        /// </summary>
        /// It's recommended to add a new property with the exact TLView subclass type when subclassing TLController.
		public TLView view { get { return _view; } }
        /// <summary>
        /// Gets the model associated to this controller.
        /// </summary>
        /// It's recommended to add a new property with the exact TLModel subclass type when subclassing TLController.
		public TLModel model { get { return _model; } }

		public TLController( TLView view, TLModel model )
		{
			_view = view;
			_model = model;
		}

        /// <summary>
        /// Called whenever a new TLView is loaded.
        /// </summary>
		public virtual void OnLoad() 
		{
			if (OnLoadActions != null) {
				OnLoadActions ();
				OnLoadActions = null;
			}
		}

        /// <summary>
        /// Update method.
        /// </summary>
        /// This method is usually overrided by subclasses to implement custom controller logic.
        /// <param name="elapsed">Elapsed time in seconds.</param>
		public virtual void Update(double elapsed) {}

        /// <summary>
        /// Actions to do when the window closes and the view is about to be destroyed
        /// </summary>
        public virtual void OnUnload() {}
	}
}
