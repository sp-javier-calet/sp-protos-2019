using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Base class for all windows that contains views.
    /// </summary>
    /// TLWindow is, in fact, an EditorWindow that has a set of interchangeable TLView.
    /// Can send key enter press events.
    /// Can send delayed close events.
    /// Can send window closed events.
    /// Can send scene changed events.
	public abstract class TLWindow : EditorWindow
	{
        /// <summary>
        /// The refresh timer. The window will force a reapaint every frame.
        /// </summary>
		static readonly float 	fps = 1f / 30f;

		List<TLView> 	        _views;
		TLView 			        _currentView;
		TLEventManager 	        _eventManager;

        // Cross window close(or more) events could be achieved by registering windows and accessing their event managers
        List<TLWindow>          _registeredWindows;

        TLEvent 		        _keyboardEnterPressed;
        /// <summary>
        /// Gets the keyboard enter pressed event to connect to.
        /// </summary>
        public TLEvent	        keyboardEnterPressed { get { return _keyboardEnterPressed; } private set { _keyboardEnterPressed = value; } }

        TLEvent                 _delayedClose;
        /// <summary>
        /// Gets the delayed close event to connect to.
        /// </summary>
        public TLEvent          delayedClose { get { return _delayedClose; } private set { _delayedClose = value; } }

        TLEvent                 _closed;
        /// <summary>
        /// Gets the window closed event to connect to.
        /// </summary>
        /// This event will be never processed by the current TLWindow so its meant to be registered to the TLWindow event managers.
        public TLEvent          closed { get { return _closed; } private set { _closed = value; } }

        TLEvent                 _sceneChanged;
        /// <summary>
        /// Gets the Unity current scene changed event.
        /// </summary>
        public TLEvent          sceneChanged { get { return _sceneChanged; } private set { _sceneChanged = value; } }

		protected TLView		_delayedChangeView;
        protected string        _currentScene;

		bool			        _resizeRequired;
		DateTime		        _lastUpdateTick;
		double			        _accumTime;

		Texture2D		        _prevTex;
		int				        _callCount;
		Event			        _prevEvent;
		string			        _prevTooltip;
        string                  _prevFocusedControl;

        System.Object 	        _eventLock;

        static TLStyle	        _tooltipStyle;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using custom tooltip.
        /// </summary>
        /// Unity editor default tooltip system doesn't work very well with the library draw pipeline. A custom defaul one is defined and can be activated.
        /// <value><c>true</c> if this instance is using custom tooltip; otherwise, <c>false</c>.</value>
		public bool		        IsUsingCustomTooltip { get; set; }
        /// <summary>
        /// Gets the current Unity scene.
        /// </summary>
        /// It's recommended to use this variable instead of using UnityEditor utilities.
        /// <value>The current scene.</value>
        public string           CurrentScene { get { return _currentScene; } }
        public string           PrevFocusedControl { get { return _prevFocusedControl; } }
        //HACK: Detect when texture in memory is erased and perform a reimport
        Texture2D               _dummyMemoryTexture;
        /// <summary>
        /// Gets a value indicating whether the textures in memory have been dropped and a general Reimport is needed.
        /// </summary>
        /// When changing Unity scenes the textures allocated in memory are dropped.
        /// <value><c>true</c> if reimport needed; otherwise, <c>false</c>.</value>
        public bool             ReimportNeeded { get { return _dummyMemoryTexture == null; } }

		/// <summary>
        /// Gets the event manager.
        /// </summary>
        /// The event manager is the point of entry to add new TLEvents. Its thread-safe so it can be accessed from outside the main thread.
        /// <value>The event manager.</value>
		public TLEventManager eventManager { 
			get 
			{ 
				lock(_eventLock)
				{
					return _eventManager;
				}
			} 
		}

		static TLWindow()
		{
			_tooltipStyle = new TLStyle("Label");
			_tooltipStyle.fontSize = 12;
			_tooltipStyle.fontStyle = FontStyle.Italic;
			_tooltipStyle.normal.background = TLEditorUtils.whiteImg;
            _tooltipStyle.normal.textColor = Color.black;
		}

		public TLWindow()
		{
			_eventManager = new TLEventManager();
            _registeredWindows = new List<TLWindow> ();
			_views = new List<TLView>();
			_eventLock = new System.Object ();

			// Current time in seconds
			_lastUpdateTick = DateTime.Now;
			_accumTime = 0;
			_resizeRequired = false;
			_callCount = 0;
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _prevFocusedControl = "";
			IsUsingCustomTooltip = false;

            _keyboardEnterPressed = new TLEvent("keyboardEnterPressed");
            _delayedClose = new TLEvent("delayedClose");
            _closed = new TLEvent("closed");
            _sceneChanged = new TLEvent("sceneChanged");

            delayedClose.Connect (Close);
		}

		public void CopyOther(TLWindow other)
		{
			this._eventManager = other._eventManager;
            this._registeredWindows = other._registeredWindows;
			this._views = other._views;
            this._currentView = other._currentView;
            this._delayedChangeView = other._delayedChangeView;
            this._keyboardEnterPressed = other._keyboardEnterPressed;
            this._delayedClose = other._delayedClose;
            this._closed = other._closed;
		}

		protected void OnGUI()
		{
            if (_resizeRequired && _currentView != null) {

				ResizeWindow (_currentView);
				_resizeRequired = false;
			}

			if ( _currentView == null ) return;

            _prevFocusedControl = GUI.GetNameOfFocusedControl ();

			_currentView.Draw();

			if (IsUsingCustomTooltip) {

				_prevTooltip = GUI.tooltip;

                if (_prevTooltip != null && _prevTooltip != String.Empty) {
    				Vector2 tooltipSize = _tooltipStyle.GetStyle ().CalcSize (new GUIContent (_prevTooltip));
    				GUILayout.BeginArea (new Rect (0, 0, (int)tooltipSize.x, (int)tooltipSize.y), _prevTooltip, _tooltipStyle.GetStyle ());
    				GUILayout.EndArea ();
                }
			}

			// Capture and send various events to be able to use then on Update cycles
			if (Event.current != null) {
				switch(Event.current.type)
				{
					case EventType.KeyUp:
						if(Event.current.keyCode == KeyCode.Return) eventManager.AddEvent(keyboardEnterPressed);
						break;
				}
			}
		}

		protected void Update()
		{
			if ( _currentView == null ) return;

            if (ReimportNeeded) {
                _dummyMemoryTexture = new Texture2D(1,1);
                Reimport ();
            }

			double elapsed = (DateTime.Now - _lastUpdateTick).TotalSeconds;
			_lastUpdateTick = DateTime.Now;

			if (_delayedChangeView != null) {
				LoadView(_delayedChangeView);
			}

			eventManager.ProcessEvents(); // use of the Property with the lock !!
			_currentView.Update(elapsed);

            if (!_currentScene.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)) {
                _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                eventManager.AddEvent(sceneChanged);
            }

			// call an explicit Repaint at a given fps
			_accumTime += elapsed;
			if (_accumTime >= TLWindow.fps) {
				_accumTime = 0;
				Repaint();
			}
		}

		/// <summary>
        /// (Internal Use Only)Resizes the window to the size of the TLView.
        /// </summary>
        /// <param name="view">View.</param>
		public void ResizeWindow( TLView view )
		{
			if ( view.minSize != null )
				minSize = view.minSize.Value;

			if ( view.maxSize != null )
				maxSize = view.maxSize.Value;

			if ( view.position != null )
				position = view.position.Value;
		}

        /// <summary>
        /// Adds the view to the view list for the window.
        /// </summary>
        /// <param name="view">View.</param>
		public void AddView( TLView view )
		{
			_views.Add( view );
			if ( _currentView == null )
				_currentView = view;
		}

        /// <summary>
        /// Inmemdiately loads the view.
        /// </summary>
        /// This method changes the currently displayed view inmediately.
        /// It's recommended to use the delayed ChangeView method instead.
        /// <param name="view">View.</param>
		public void LoadView( TLView view )
		{
			if ( !_views.Contains( view ) )
				throw new UnityException( "The windows does not contains required view" );

			_currentView = view;
			_currentView.OnLoad();
			_resizeRequired = true;
			_delayedChangeView = null;
			this.Repaint();
		}

        /// <summary>
        /// (Testing)Captures window area and saves it with a name and number if there are previous files with the same name.
        /// </summary>
        /// <param name="folder">Output folder.</param>
        /// <param name="fname">Output file name.</param>
		public void CaptureScreen(string folder, string fname)
		{
			string fpath = folder + "/" + _callCount.ToString() + "_" + fname + ".png";
			_prevTex = new Texture2D ((int)this.position.width, (int)this.position.height);
			Rect nRect = new Rect (this.position);
			nRect.x = 0;
			nRect.y = 0;
			_prevTex.ReadPixels (nRect, 0, 0);
			byte[] bytes = _prevTex.EncodeToPNG ();
			/*
			int fileCount = -1;
			do
			{
				fileCount++;
			}
			while (File.Exists(fpath + (fileCount > 0 ? "(" + fileCount.ToString() + ").png" : ".png")));
			
			//Not create a file
			var fs = File.Create(fpath + (fileCount > 0 ? "(" + (fileCount).ToString() + ").png" : ".png"));
			Debug.Log (fs.Name);
			*/
			var fs = File.Create(fpath);
			fs.Write (bytes,0,bytes.Length);
			fs.Close ();
			++_callCount;
		}

        /// <summary>
        /// Gets the current Unity scene name without the leading path and extension.
        /// </summary>
        /// <returns>The current scene name no ext.</returns>
        public string GetCurrentSceneNameNoExt()
        {
            return Path.GetFileNameWithoutExtension(CurrentScene);
        }

        /// <summary>
        /// Schedules a delayed close for the window.
        /// </summary>
        /// The close action will be performed in the next Update call.
        public void DelayedClose()
        {
            eventManager.AddEvent(delayedClose);
        }

        /// <summary>
        /// Adds a TLEvent to the event manager of this window as well as to all windows registered to this one.
        /// </summary>
        /// <param name="e">The event.</param>
        public void AddGlobalEvent(TLAbstractEvent e)
        {
            eventManager.AddEvent(e);
            foreach(TLWindow registeredWindow in _registeredWindows) {
                if (registeredWindow != null){
                    registeredWindow.eventManager.AddEvent(e);
                }
            }
        }

        /// <summary>
        /// Registers another window to this one.
        /// </summary>
        /// This allows to have more than one window(pop-ups?) at the same time and pass global events between them using the AddGlobalEvent method.
        /// This way it's possible to detect the closure of another window and react to it.
        /// <param name="window">Window.</param>
        public void RegisterWindow(TLWindow window)
        {
            foreach (TLWindow registeredWindow in _registeredWindows){
                if (window == registeredWindow){
                    return;
                }
            }

            _registeredWindows.Add(window);
        }

        /// <summary>
        /// Unregister a previously registered window.
        /// </summary>
        /// <param name="window">Window.</param>
        public void UnRegisterWindow(TLWindow window)
        {
            for (int i = 0; i < _registeredWindows.Count; ++i) {
                var registeredWindow = _registeredWindows[i];
                if (window == registeredWindow) {
                    _registeredWindows.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Close this window inmediately.
        /// </summary>
        /// <param name="supressEvent">If set to <c>true</c> no closed event will be sent. A global closed event will be sent otherwise.</param>
        virtual public void Close(bool supressEvent=false)
        {
            if (!supressEvent) {
                AddGlobalEvent(closed);
            }

            base.Close();
        }

        /// <summary>
        /// Call to Reimport to the basic texture tracking systems of the library.
        /// </summary>
        /// TLTexturePool and TLEditorUtils will be reimported because their memory allocated texture have been destroyed.
        /// TLIcons do not need no be reimported because the textures are saved as Assets and are automatically reimported by Unity.
        /// If a custom memory texture pool system is added, it will need to be reimported in this method(overriden) as well.
        protected virtual void Reimport ()
        {
            //Try to refresh TLTexturePool textures as atlas textures, that are temporary, have been destroyed
            TLTexturePool.Reimport();
            
            //Refresh TL Images too
            TLEditorUtils.Reimport();
        }

		/// <summary>
        /// Schedules a view change.
        /// </summary>
        /// Its not inmediate and will be called in the next Update cycle.
        /// This is the preferred method to change views from the controllers.
        /// <param name="view">View.</param>
		public void ChangeView( TLView view )
		{
			Debug.Log ("Delayed change");
			_delayedChangeView = view;
		}

		void OnDestroy()
		{
			for (int i = 0; i < _views.Count; i++) {
				_views [i].OnDestroy ();
			}
		}
	}
}
