using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using SocialPoint.Base;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.Social
{
    public class AdminPanelFacebook : IAdminPanelConfigurer, IAdminPanelGUI
    {
        AdminPanel.AdminPanel _adminPanel;
        IFacebook _facebook;
        Toggle _loggedInToggle;
        Text _userText;
        Text _friendsText;
        RawImage _userPhoto;

        public AdminPanelFacebook(IFacebook facebook)
        {
            _facebook = facebook;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_facebook == null)
            {
                return;
            }
            _adminPanel = adminPanel;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Facebook", this));


            var cmd = new ConsoleCommand()
                .WithDescription("do a facebook graph apy query")
                    .WithOption(new ConsoleCommandOption("0|method")
                                .withDescription("query method"))
                    .WithOption(new ConsoleCommandOption("1|path")
                                .withDescription("query path"))
                    .WithDelegate(OnQueryCommand);
            adminPanel.RegisterCommand("fb-query", cmd);
        }

        void OnQueryCommand(ConsoleCommand cmd)
        {
            var query = new FacebookGraphQuery();

            query.Method = (FacebookGraphQuery.MethodType) Enum.Parse(typeof(FacebookGraphQuery.MethodType), cmd["method"].Value, true);
            var uri = new Uri("http://localhost" + cmd["path"].Value);
            query.Path = uri.AbsolutePath;
            query.Params = new UrlQueryAttrParser().ParseString(uri.Query).AsDic;

            _facebook.QueryGraph(query, (_, err) => {
                if(Error.IsNullOrEmpty(err))
                {
                    var serializer = new JsonAttrSerializer();
                    serializer.PrettyPrint = true;
                    var json = serializer.SerializeString(query.Response);
                    PrintLog(string.Format("query response {0}", json));
                }
                else
                {
                    PrintLog(string.Format("query error {0}", err));
                }
            });
        }

        private void PrintLog(string msg)
        {
            _adminPanel.Console.Print(string.Format("Facebook: {0}", msg));
        }

        private void PrintError(string what, Error err)
        {
            if(Error.IsNullOrEmpty(err))
            {
                PrintLog(string.Format("success when {0}", what));
            }
            else
            {
                PrintLog(string.Format("error when {0}: {1}", what, err));
            }
        }

        void UpdateLoggedIn()
        {
            var ev = _loggedInToggle.onValueChanged;
            _loggedInToggle.onValueChanged = new Toggle.ToggleEvent();
            _loggedInToggle.isOn = _facebook.IsConnected;
            _loggedInToggle.onValueChanged = ev;

            if(_facebook.User != null)
            {
                _userText.text = _facebook.User.ToString();
            }
            else
            {
                _userText.text = string.Empty;
            }
            if(_facebook.Friends != null)
            {
                var friends = new string[_facebook.Friends.Count];
                var i = 0;
                foreach(var friend in _facebook.Friends)
                {
                    friends[i++] = friend.ToString();
                }
                _friendsText.text = string.Join("\n", friends);
            }
            else
            {
                _friendsText.text = string.Empty;
            }

            if(_facebook.User != null)
            {
                _facebook.LoadPhoto(_facebook.User.UserId, (tex, err) => {
                    if(Error.IsNullOrEmpty(err) && tex != null)
                    {
                        _userPhoto.texture = tex;
                    }
                });
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Facebook");

            layout.CreateMargin();

            layout.CreateLabel("Facebook User");

            var hlayout = layout.CreateHorizontalLayout();
            _userPhoto = hlayout.CreateImage(new Vector2(100,100));
            _userText = hlayout.CreateVerticalScrollLayout().CreateTextArea();

            layout.CreateLabel("Facebook Friends");
            _friendsText = layout.CreateVerticalScrollLayout().CreateTextArea();

            _loggedInToggle = layout.CreateToggleButton("Logged In", _facebook.IsConnected, (status) => {
                if(status)
                {
                    _facebook.Login((err) => {
                        UpdateLoggedIn();
                        PrintError("logging in", err);
                    });
                }
                else
                {
                    _facebook.Logout((err) => {
                        UpdateLoggedIn();
                        PrintError("logging out", err);
                    });
                }
            });

            layout.CreateButton("Send App Request", () => {
                var req = new FacebookAppRequest();
                req.Message = "Test message";
                _facebook.SendAppRequest(req, (_, err) => {
                    PrintError("sending app request", err);
                });
            });

            layout.CreateButton("Post on Wall", () => {
                var post = new FacebookWallPost();
                _facebook.PostOnWallWithDialog(post, (_, err) => {
                    PrintError("posting on wall", err);
                });
            });

            UpdateLoggedIn();
        }
    }
}
