using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using SocialPoint.Tool.Shared;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class AuthController : TLController
    {
        public AuthView View { get { return (AuthView)_view; } }
        public AuthModel Model { get { return (AuthModel)_model; } }

        Thread 					logger_thread;
        SPAMAuthenticator 		spamAuthenticator;

        // Events
        TLEvent<AuthResponse>               responseRecievedEvent;
        public TLEvent                      tryWebLoginEvent { get; private set; }
        public TLEvent<SPAMAuthenticator>   successfulAuthEvent { get; private set; }
		
        public AuthController(TLView view, TLModel model): base ( view, model )
        {
            Init();
        }

        void Init()
        {
            Abort();

            logger_thread = null;
            spamAuthenticator = new SPAMAuthenticator();
            responseRecievedEvent = new TLEvent<AuthResponse>("ResponseRecieved");
            tryWebLoginEvent = new TLEvent("TryWebLoginEvent");
            successfulAuthEvent = new TLEvent<SPAMAuthenticator>("SuccessfulAuthEvent");

            responseRecievedEvent.Connect(OnResponseReceived);
        }

        public override void OnLoad()
        {
            // in miliseconds
            int LOGIN_TIMEOUT = 10 * 1000;
			// open TCP ports
			var socketPermission = new SocketPermission (NetworkAccess.Accept, TransportType.Tcp, SPAMAuthenticator.SPAM_SERVICES_HOSTNAME, SPAMAuthenticator.SOCKET_LOCAL_PORT);
			socketPermission.Demand ();

            ThreadStart starter = delegate {
                try
                {
                    var response = spamAuthenticator.TryCachedAuthentication(LOGIN_TIMEOUT);
                    responseRecievedEvent.Send(View.window, AuthResponse.FromAttr(response));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            };
            logger_thread = new Thread(starter);

            logger_thread.Start();

            base.OnLoad();
        }

        public override void Update(double elapsed)
        {
            if(View.lbloadingDots.IsVisible)
            {

                // Compute dynamic dots text
                DateTime curr_time = DateTime.Now;
                TimeSpan elapsed_time = curr_time - Model.lastSecondTime;
			
                if(elapsed_time.TotalSeconds >= 1.0)
                {
                    Model.numLoadingDots = Model.numLoadingDots == 3 ? 0 : Model.numLoadingDots + 1;
                    Model.lastSecondTime = curr_time;

                    string dotsText = "" + string.Concat(Enumerable.Repeat(".", Model.numLoadingDots).ToArray());
                    View.lbloadingDots.text = dotsText;

                    View.window.Repaint();
                }
            }
        }

        public void OnResponseReceived(AuthResponse response)
        {
            //Save credentials used no mater what
            spamAuthenticator.SaveLoginPrefs();

            if(!response.Success)
            {
                OnFailedAuth(response.IsRecoverable, response.message);
            }
            else
            {
                OnSuccessfulAuth();
            }

            View.window.Repaint();
        }

        public void OnFailedAuth(bool isRecoverable, string errMessage)
        {
            if(!isRecoverable)
            {
                View.icoLogginStatus.SetTexture(TLIcons.failImg);
                View.icoLogginStatus.SetVisible(true);
                View.lbLogginMessage.text = errMessage;
                View.lbloadingDots.SetVisible(false);
            }
            else
            {
                tryWebLoginEvent.Send(View.window);
                View.window.Close();
            }
        }

        public void OnSuccessfulAuth()
        {
            View.icoLogginStatus.SetTexture(TLIcons.successImg);
            View.icoLogginStatus.SetVisible(true);
            View.lbLogginMessage.text = "Success";
            View.lbloadingDots.SetVisible(false);

            successfulAuthEvent.Send(View.window, spamAuthenticator);
            View.window.Close();
        }

        public void Abort()
        {
            if(logger_thread != null && logger_thread.IsAlive)
            {
                logger_thread.Abort();
            }
        }
    }
}
