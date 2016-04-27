using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;

namespace SocialPoint.Tool.Shared.TLGUI
{
	/// <summary>
	/// Web Browser embedded in a view.
	/// </summary>
    /// In fact, the Web Browser IS the whole TLWindow because internally reflection is used to cast the ScriptableObject class WebVew(used by the AssetStore) 
    /// into the current window so there cannot be other Widgets when this view is active.
    /// Can send response received events.
    /// Can send url changed events.
#if UNITY_5
    public class TLWWebBrowser : TLWidget
    {
        static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.SetField;
        static StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

        //protected TLView View;

        protected EditorWindow _winPtr;
        Type _winT;
        object _webView;
        Type _webViewT;
        
        MethodInfo _loadUrlM;
        
        Queue pendingGoToUrl;
        
        string _url;
        /// <summary>
        /// Gets the current URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get { return _url; } }
        
        private TLEvent _onResponseReceivedEvent;
        /// <summary>
        /// Gets the response received event to connect to.
        /// </summary>
        /// <value>The response received event to connect to.</value>
        public TLEvent onResponseReceivedEvent { get { return _onResponseReceivedEvent; } }
        
        private TLEvent _urlChangedEvent;
        /// <summary>
        /// Gets the url changed event to connect to.
        /// </summary>
        /// <value>The url changed event to connect to.</value>
        public TLEvent urlChangedEvent { get { return _urlChangedEvent; } }
        
        public TLWWebBrowser( TLView view, string name ) : base(view,name)
        {
            //View = view;
            pendingGoToUrl = new Queue ();
            pendingGoToUrl.Enqueue ("http://www.google.com");
            _url = "http://www.google.com";
        }
        
        public TLWWebBrowser( string url, TLView view, string name ) : base(view,name)        {

            //View = view;
            pendingGoToUrl = new Queue ();
            pendingGoToUrl.Enqueue (url);
            _url = url;
        }
        
        public void Init()
        {
            _onResponseReceivedEvent = new TLEvent( "OnResponseReceived" );
            _urlChangedEvent = new TLEvent( "urlChanged" );
            
            Type webType = GetTypeFromAllAssemblies("WebViewEditorWindow");
            var createMethod = webType.GetMethod("CreateBase", fullBinding);            

            int minWidth = 400;
            int minHeight = 400;
            int maxWidth = 1000;
            int maxHeight = 1000;
            _winPtr = createMethod.Invoke(null, new object[] {"WebBrowser", _url, minWidth, minHeight, maxWidth, maxHeight}) as EditorWindow;
            

            _winT = GetTypeFromAllAssemblies("WebViewEditorWindow");
            _webViewT = GetTypeFromAllAssemblies("WebView");
            _loadUrlM = _webViewT.GetMethod("LoadURL", fullBinding);
        }
        
        void InitWebView()
        {
            _webView = _winT.GetField("webView", fullBinding).GetValue(_winPtr);
            
            _winPtr.wantsMouseMove= true;
            Application.logMessageReceived += CallbackHndl;
        }
        
        void CallbackHndl(string logString, string stackTrace, LogType type)
        {
            View.window.eventManager.AddEvent( onResponseReceivedEvent );
            Application.logMessageReceived -= CallbackHndl;
        }
        
        public void GoToUrl( string url )
        {
            foreach (string pending in pendingGoToUrl)
                if (pending.Equals (url))
                    return;
            pendingGoToUrl.Enqueue (url);
        }

        //public void Update()
        //{
        //    if (_webView != null)
        //    {
        //        if (pendingGoToUrl.Count > 0)
        //        {
        //            var pendingUrl = pendingGoToUrl.Dequeue() as String;
        //            if (!pendingUrl.Equals(Url))
        //            {
        //                _loadUrlM.Invoke(_webView, new object[] { pendingUrl });
        //                _url = pendingUrl;
        //            }

        //        }
        //    }

        //}

        //void OnGUI()
        //{
        //    if (_webView == null)
        //    {
        //        InitWebView();
        //    }
        //}

        //void OnDestroy()
        //{
        //    if (_winPtr != null)
        //    {
        //        _winPtr.Close();
        //    }
        //}


        public override void Update(double elapsed)
        {
            if (_webView != null)
            {
                if (pendingGoToUrl.Count > 0)
                {
                    var pendingUrl = pendingGoToUrl.Dequeue() as String;
                    if (!pendingUrl.Equals(Url))
                    {
                        _loadUrlM.Invoke(_webView, new object[] { pendingUrl });
                        _url = pendingUrl;
                    }

                }
            }

            base.Update(elapsed);
        }

        public override void Perform()
        {
            if (_webView == null)
            {
                InitWebView();
            }

            base.Perform();
        }

        public override void OnDestroy()
        {
            if (_winPtr != null)
            {
                _winPtr.Close();
            }
        }

        string GetCurrentUrl()
        {
            return _url;
        }
        
        
        static Type GetTypeFromAllAssemblies(string typeName) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies) {
                Type[] types = assembly.GetTypes();
                foreach(Type type in types) {
                    if(type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
                        return type;
                }
            }
            return null;
        }
    }
#else
	public class TLWWebBrowser : TLWidget 
	{
		static BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
		static StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

		object webView;
		Type webViewType;

		MethodInfo doGUIMethod;
		MethodInfo loadURLMethod;
		MethodInfo focusMethod;
		MethodInfo unFocusMethod;
		PropertyInfo needsRepaintProperty;

        object windowScript;
        Type windowScriptType;
        MethodInfo windowScriptEvalMethod;
        MethodInfo windowScriptGetAsJsonMethod;

		Vector2 resizeStartPos;
		Rect resizeStartWindowSize;
		MethodInfo dockedGetterMethod;

        Queue pendingGoToUrl;

		string _url;
        /// <summary>
        /// Gets the current URL.
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get { return _url; } }

        private TLEvent _onResponseReceivedEvent;
        /// <summary>
        /// Gets the response received event to connect to.
        /// </summary>
        /// <value>The response received event to connect to.</value>
		public TLEvent onResponseReceivedEvent { get { return _onResponseReceivedEvent; } }

        private TLEvent _urlChangedEvent;
        /// <summary>
        /// Gets the url changed event to connect to.
        /// </summary>
        /// <value>The url changed event to connect to.</value>
        public TLEvent urlChangedEvent { get { return _urlChangedEvent; } }

		public TLWWebBrowser( TLView view, string name ) : base(view, name) 
		{
            pendingGoToUrl = new Queue ();
            pendingGoToUrl.Enqueue ("http://www.google.com");
			_url = "http://www.google.com";
		}

		public TLWWebBrowser( string url, TLView view, string name ) : base(view, name) 
		{
            pendingGoToUrl = new Queue ();
            pendingGoToUrl.Enqueue (url);
			_url = url;
		}

		public void Init()
		{
			_onResponseReceivedEvent = new TLEvent( "OnResponseReceived" );
            _urlChangedEvent = new TLEvent( "urlChanged" );
			//Get WebView type
			webViewType = GetTypeFromAllAssemblies("WebView");
            windowScriptType = GetTypeFromAllAssemblies("WebScriptObject");
			//Get docked property getter MethodInfo
			dockedGetterMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
		}

		void InitWebView()
		{
			webView = ScriptableObject.CreateInstance(webViewType);
			webViewType.GetMethod("InitWebView").Invoke(webView, new object[] {(int)View.position.Value.width,(int)View.position.Value.height,false});
			webViewType.GetMethod("set_hideFlags").Invoke(webView, new object[] {13});
			
			loadURLMethod = webViewType.GetMethod("LoadURL");

			loadURLMethod.Invoke(webView, new object[] { _url });
			webViewType.GetMethod("SetDelegateObject").Invoke(webView, new object[] {View.window});

			doGUIMethod = webViewType.GetMethod("DoGUI");
			focusMethod = webViewType.GetMethod("Focus");
			unFocusMethod = webViewType.GetMethod("UnFocus");
			needsRepaintProperty = webViewType.GetProperty ("needsRepaint");

            var windowScriptProperty = webViewType.GetProperty("windowScriptObject");
            windowScript = windowScriptProperty.GetValue (webView, null);
            windowScriptEvalMethod = windowScriptType.GetMethod("EvalJavaScript");
            windowScriptGetAsJsonMethod = windowScriptType.GetMethod("GetAsJSON", new [] {typeof(string), typeof(int)});
			
			View.window.wantsMouseMove = true;
			Application.RegisterLogCallback (CallbackHndl);
		}

		void CallbackHndl(string logString, string stackTrace, LogType type)
		{
			View.window.eventManager.AddEvent( onResponseReceivedEvent );
			Application.RegisterLogCallback(null);
		}

        /// <summary>
        /// Enqueue a navigate command to the given url.
        /// </summary>
        /// <param name="url">url.</param>
        /// This action is not inmediate, a url changed event will be fired when the action is performed.
		public void GoToUrl( string url )
		{
            foreach (string pending in pendingGoToUrl)
                if (pending.Equals (url))
                    return;
            pendingGoToUrl.Enqueue (url);
		}

		static Type GetTypeFromAllAssemblies(string typeName) {
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach(Assembly assembly in assemblies) {
				Type[] types = assembly.GetTypes();
				foreach(Type type in types) {
					if(type.Name.Equals(typeName, ignoreCase) || type.Name.Contains('+' + typeName)) //+ check for inline classes
						return type;
				}
			}
			return null;
		}

		public override void Update(double elapsed) 
		{
            if (webView!=null) {
                if (pendingGoToUrl.Count > 0) {
                    var pendingUrl = pendingGoToUrl.Dequeue () as String;
                    if (!pendingUrl.Equals(Url))
                        loadURLMethod.Invoke(webView, new object[] { pendingUrl });
                }

                string url = GetCurrentUrl();

                if (!url.Equals(_url)) {
                    _url = url;
                    View.window.eventManager.AddEvent( urlChangedEvent );
                }

                if ((bool)(needsRepaintProperty.GetValue (webView, null)))
                    View.window.Repaint ();
            }
		}

		public override void Perform()
		{
            if (webView == null)
                //Init web view
                InitWebView();

			if(GUI.GetNameOfFocusedControl().Equals("urlfield"))
				unFocusMethod.Invoke(webView, null);

			object dockedResponse = dockedGetterMethod.Invoke (View.window, null);
			bool isDocked = dockedResponse != null ? (bool)dockedResponse : false;

			Rect webViewRect = new Rect(0,20,View.position.Value.width,View.position.Value.height - (isDocked ? 20 : 40));
			if(Event.current.isMouse && Event.current.type == EventType.MouseDown && webViewRect.Contains(Event.current.mousePosition)) {
				GUI.FocusControl("hidden");
				focusMethod.Invoke(webView, null);
			}
			
			//Hidden, disabled, button for taking focus away from urlfield
			GUI.enabled = false;
			GUI.SetNextControlName("hidden");
			GUI.Button(new Rect(-20,-20,5,5), string.Empty);
			GUI.enabled = true;
			
			//Web view
			if (webView != null) {
				doGUIMethod.Invoke (webView, new object[] {webViewRect});
			}
		}

		public override void OnDestroy()
		{
			//Destroy web view
			if (webViewType != null) {
				webViewType.GetMethod ("DestroyWebView", fullBinding).Invoke (webView, null);
				webView = null;
			}
		}

        string GetCurrentUrl()
        {
            string locationContent = windowScriptGetAsJsonMethod.Invoke(windowScript, new object[] { "location", 0 }) as String;
            if (!locationContent.Equals("{}")) {
                var location = windowScriptEvalMethod.Invoke(windowScript, new object[] { "location;" });
                var href = windowScriptGetAsJsonMethod.Invoke(location, new object[] { "href", 0 }) as String;

                // remove " characters
                return href.Substring(1, href.Length - 2);
            } else {

                return this.Url;
            }
        }
	}
#endif
}

