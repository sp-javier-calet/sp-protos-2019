using UnityEditor;
using UnityEngine;
using SocialPoint.Tool.Shared;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class BRGuiWindow : TLWindow
    {
        static BRGuiWindow          _instance;
        public static BRGuiWindow   instance { get { return _instance; } private set { _instance = value; } }

        public BRView               brView;


        public void Init()
        {
            titleContent.text = "SPAM";
            IsUsingCustomTooltip = true;

            var brModel = ScriptableObject.CreateInstance<BRModel>();

            brView = new BRView(this, brModel);
            AddView(brView);
            brView.SetController(new BRController(brView, brModel));

            StartAuthWorkflow();
        }

        [MenuItem("SPAM/Bundle Request")]
        public static void GetOrCreateWindow()
        {
            if(instance == null)
            {
                instance = ScriptableObject.CreateInstance<BRGuiWindow>();
                instance.Show();
            }
            else
            {
                instance.Focus();
            }
        }

        public void OnEnable()
        {
            if(instance == null)
            {
                instance = this;
                instance.Init();
                instance.LoadView(instance.brView);
            }
        }

        void StartAuthWorkflow()
        {
            //Cached Auth
            var authWindow = ScriptableObject.CreateInstance<AuthGuiWindow> ();
            //var dim = authWindow.authView.position;

            //authWindow.authView.position = new Rect(center.x - dim.Value.width/2,
            //                                        center.y - dim.Value.height/2,
            //                                        dim.Value.width,
            //                                        dim.Value.height);
            //authWindow.RegisterWindow(this);
            authWindow.authView.Controller.tryWebLoginEvent.Connect(TryWebLogin);
            authWindow.authView.Controller.successfulAuthEvent.Connect(brView.Controller.OnAuthentication);
        }

        void TryWebLogin()
        {
            //var center = brView.position.Value.center;

            //Web Login
            var webLoginWindow = new WebLoginBrowser(brView, "webBrowser");

            brView.AddWidget(webLoginWindow);
            //var dim = webLoginWindow.webLoginView.position;

            //webLoginWindow.webLoginView.position = new Rect(center.x - dim.Value.width/2,
            //                                                center.y - dim.Value.height/2,
            //                                                dim.Value.width,
            //                                                dim.Value.height);

            //webLoginWindow.RegisterWindow(this);
            webLoginWindow.successfulLoginEvent.Connect(brView.Controller.OnAuthentication);
        }
    }
}
