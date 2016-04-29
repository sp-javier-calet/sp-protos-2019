using UnityEngine;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public class AuthGuiWindow : TLWindow
    {
        static AuthGuiWindow            _instance;
        public static AuthGuiWindow     instance { get { return _instance; } private set { _instance = value; } }

        public AuthView                 authView    { get; private set; }

        public void Init()
        {
            titleContent.text = "SPAM Auto Login";

            var authModel = ScriptableObject.CreateInstance<AuthModel>();

            authView = new AuthView(this, authModel);
            AddView(authView);
            authView.SetController(new AuthController(authView, authModel));
        }

        public void OnEnable()
        {
            if(instance == null)
            {
                instance = this;
                instance.Init();
                instance.LoadView(instance.authView);
                instance.ShowUtility();
            }
        }
    }
}

