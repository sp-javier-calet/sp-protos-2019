using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;
using System.Threading;
using SocialPoint.Tool.Shared;
using System.Net;
using System;
using UnityEditor;

namespace SocialPoint.Editor.SPAMGui
{
    public class WebLoginBrowser : TLWWebBrowser
    {
        Thread logger_thread;
        SPAMAuthenticator spamAuthenticator;

        // Events
        TLEvent<AuthResponse> responseRecievedEvent;
        public TLEvent<bool> webLoginResultEvent { get; private set; }
        public TLEvent<SPAMAuthenticator> successfulLoginEvent { get; private set; }
        

        public WebLoginBrowser(TLView view, string name) : base(view, name) 
        {
            Init();
            Start();
        }

        public WebLoginBrowser(string url, TLView view, string name) : base(url,view, name)
        {
            Init();
            Start();
        }
                
        void Start()
        {
            Abort();

            logger_thread = null;
            spamAuthenticator = new SPAMAuthenticator();
            responseRecievedEvent = new TLEvent<AuthResponse>("ResponseRecieved");
            webLoginResultEvent = new TLEvent<bool>("WebLoginResultEvent");
            successfulLoginEvent = new TLEvent<SPAMAuthenticator>("SuccessfulLoginEvent");

            responseRecievedEvent.Connect(OnResponseReceived);
            StartLogin();
        }
        

        void StartListening()
        {
            // in miliseconds
            int LOGIN_TIMEOUT = 50 * 1000;
            // open TCP ports
            var socketPermission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, SPAMAuthenticator.SPAM_SERVICES_HOSTNAME, SPAMAuthenticator.SOCKET_LOCAL_PORT);
            socketPermission.Demand();

            ThreadStart starter = delegate {
                try
                {
                    var response = spamAuthenticator.WebAuthentication(LOGIN_TIMEOUT);
                    responseRecievedEvent.Send(View.window, AuthResponse.FromAttr(response));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };
            logger_thread = new Thread(starter);

            logger_thread.Start();
        }

        public void StartLogin()
        {
            StartListening();

            urlChangedEvent.Connect(OnWebBrowserUrlChanged);

            GoToUrl(SPAMAuthenticator.GetLoginAuthenticationURI());
        }

        public void StartLogout()
        {
            onResponseReceivedEvent.Connect(OnWebBrowserLogoutResponse);

            GoToUrl(SPAMAuthenticator.GetLogoutURI());
        }

        public void OnWebBrowserLogoutResponse()
        {
            spamAuthenticator = new SPAMAuthenticator();
            spamAuthenticator.ResetLoginPrefs();
            _winPtr.Close();
            //View.window.Close();
        }

        public void OnWebBrowserUrlChanged()
        {
            // Hack to check if browser url is in the index and force successfulLogin. This happens because somethimes OAuth doesn't respect the 'next' parameter
            if (Url.Equals(SPAMAuthenticator.SPAM_SERVICES_ENDPOINT) ||
                Url.Equals(SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + '/'))
            {
                // Navigate to successfulLogin
                GoToUrl(SPAMAuthenticator.GetCookieAuthenticationURI());
            }
        }

        public void OnResponseReceived(AuthResponse response)
        {
            if (!response.Success)
            {
                Debug.Log("<color=\"red\">Bad loggin</color>");

                OnWebLogin(response.IsRecoverable, response.message);
            }
            else
            {
                OnSuccessfulWebLogin();
            }
        }

        public void OnWebLogin(bool isRecoverable, string errMessage)
        {
            // If the login error is recoverable, start listening again
            if (isRecoverable)
            {
                StartListening();
            }
            else
            {
                webLoginResultEvent.Send(View.window, false);

                EditorUtility.DisplayDialog("Failed login", errMessage, "Close");
                _winPtr.Close();
                //View.window.Close();
            }
        }

        public void OnSuccessfulWebLogin()
        {
            webLoginResultEvent.Send(View.window, true);

            spamAuthenticator.SaveLoginPrefs();
            Debug.Log("<color=\"green\">Loggin OK</color>");

            successfulLoginEvent.Send(View.window, spamAuthenticator);
            _winPtr.Close();
            //View.window.Close();
        }

        public void Abort()
        {
            if (logger_thread != null && logger_thread.IsAlive)
            {
                logger_thread.Abort();
            }
        }


        //static WebLoginGuiWindow                _instance;
        //public static WebLoginGuiWindow         instance { get { return _instance; } private set { _instance = value; } }

        //public WebLoginView                     webLoginView { get; private set; }

        //public void Init()
        //{
        //    title = "SPAM Interactive login";

        //    var webLoginModel = ScriptableObject.CreateInstance<WebLoginModel>();

        //    webLoginView = new WebLoginView(this, webLoginModel);
        //    AddView(webLoginView);
        //    webLoginView.SetController(new WebLoginController(webLoginView, webLoginModel));
        //}

        //public void OnEnable()
        //{
        //    if(instance == null)
        //    {
        //        instance = this;
        //        instance.Init();
        //        instance.LoadView(instance.webLoginView);
        //        instance.Show();
        //    }
        //}
    }
}

