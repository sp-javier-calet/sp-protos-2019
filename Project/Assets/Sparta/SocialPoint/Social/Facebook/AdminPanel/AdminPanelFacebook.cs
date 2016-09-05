using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using SocialPoint.Base;
using SocialPoint.Attributes;
using System;
using System.Text;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public sealed class AdminPanelFacebook : IAdminPanelConfigurer, IAdminPanelGUI
    {
        AdminPanel.AdminPanel _adminPanel;
        readonly IFacebook _facebook;
        bool _loginWithUi = true;
        bool _enabledLoggedOutInteraction;
        Toggle _loggedInToggle;
        Text _userText;
        RawImage _userPhoto;

        readonly AdminPanelFacebookLoginConfig _loginConfigPanel;
        readonly AdminPanelFacebookFriends _friendsPanel;

        public AdminPanelFacebook(IFacebook facebook)
        {
            _facebook = facebook;

            _loginConfigPanel = new AdminPanelFacebookLoginConfig(facebook);
            _friendsPanel = new AdminPanelFacebookFriends(facebook);
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

            query.Method = (FacebookGraphQuery.MethodType)Enum.Parse(typeof(FacebookGraphQuery.MethodType), cmd["method"].Value, true);
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

        void PrintLog(string msg)
        {
            _adminPanel.Console.Print(string.Format("Facebook: {0}", msg));
        }

        bool PrintError(string what, Error err)
        {
            if(Error.IsNullOrEmpty(err))
            {
                PrintLog(string.Format("success when {0}", what));
                return false;
            }
            PrintLog(string.Format("error when {0}: {1}", what, err));
            return true;
        }

        void UpdateLoggedIn()
        {
            var ev = _loggedInToggle.onValueChanged;
            _loggedInToggle.onValueChanged = new Toggle.ToggleEvent();
            _loggedInToggle.isOn = _facebook.IsConnected;
            _loggedInToggle.onValueChanged = ev;

            _userText.text = string.Empty;
            if(_facebook.User != null)
            {
                var builder = new StringBuilder();
                var user = _facebook.User;

                builder.Append("Name: ").AppendLine(user.Name)
                    .Append("Id: ").AppendLine(user.UserId)
                    .Append("Token: ").AppendLine(user.AccessToken)
                    .AppendFormat("Uses App: {0}", user.UsesApp).AppendLine()
                    .Append("Profile Image: ").AppendLine(user.PhotoUrl)
                    .AppendFormat("Friends: {0}", _facebook.Friends.Count);
                _userText.text = builder.ToString();

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

            bool enabled = _facebook.IsConnected || _enabledLoggedOutInteraction;

            // Login
            _loggedInToggle = layout.CreateToggleButton("Login", _facebook.IsConnected, status => {
                if(status)
                {
                    _facebook.Login(err => {
                        layout.Refresh();
                        PrintError("logging in", err);
                    }, _loginWithUi);
                }
                else
                {
                    _facebook.Logout(err => {
                        layout.Refresh();
                        PrintError("logging out", err);
                    });
                }
            });

            layout.CreateToggleButton("Login with UI", _loginWithUi, status => {
                _loginWithUi = status;
            });
            layout.CreateOpenPanelButton("Login Configuration", _loginConfigPanel);
            layout.CreateToggleButton("Work without login", _enabledLoggedOutInteraction, status => {
                _enabledLoggedOutInteraction = status;
                layout.Refresh();
            });
            layout.CreateMargin();

            // User Info
            layout.CreateLabel("User Info");
            var hlayout = layout.CreateHorizontalLayout();
            _userPhoto = hlayout.CreateImage(new Vector2(100, 100));
            _userText = hlayout.CreateVerticalScrollLayout().CreateTextArea();
            layout.CreateMargin();

            UpdateLoggedIn();

            // Facebook features
            layout.CreateOpenPanelButton("Friends", _friendsPanel, enabled);

            layout.CreateButton("Send App Request", () => {
                var req = new FacebookAppRequest();
                req.Message = "Test message";
                _facebook.SendAppRequest(req, (_, err) => {
                    if(!PrintError("sending app request", err))
                    {
                        PrintLog(req.ToString());
                    }
                });
            }, enabled);

            layout.CreateButton("Post on Wall", () => {
                var post = new FacebookWallPost();
                _facebook.PostOnWallWithDialog(post, (_, err) => {
                    if(!PrintError("posting on wall", err))
                    {
                        PrintLog(post.ToString());
                    }
                });
            }, enabled);
        }

        public sealed class AdminPanelFacebookLoginConfig : IAdminPanelGUI
        {
            static readonly List<string> PublicProfilePermissions = new List<string> {"public_profile"};
            static readonly List<string> EmailPermissions = new List<string> {"email"};
            static readonly List<string> UserFriendsPermissions = new List<string> {"user_friends"};

            readonly IFacebook _facebook;

            public AdminPanelFacebookLoginConfig(IFacebook facebook)
            {
                _facebook = facebook;
            }

            void LoginPermissionButton(AdminPanelLayout layout, string name, List<string> permissionAsList)
            {
                var permission = permissionAsList.First();
                bool active = _facebook.LoginPermissions.Contains(permission);

                layout.CreateToggleButton(name, active, status =>
                    {
                        if(status)
                        {
                            _facebook.LoginPermissions.Add(permission);
                        }
                        else
                        {
                            _facebook.LoginPermissions.Remove(permission);
                        }
                    });
            }

            void AskForPermissionButton(AdminPanelLayout layout, string name, List<string> permissionAsList)
            {
                layout.CreateToggleButton(name, _facebook.HasPermissions(permissionAsList), status =>
                    {
                        if(status)
                        {
                            _facebook.AskForPermissions(permissionAsList, (permissions, err) =>
                                {
                                    if(!Error.IsNullOrEmpty(err))
                                    {
                                        layout.AdminPanel.Console.Print("Error when asking for permissions. " + err.Msg);
                                    }
                                    layout.Refresh();
                                });
                        }
                    });
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Facebook Configuration");
                layout.CreateMargin();

                layout.CreateLabel("Login Permissions");
                LoginPermissionButton(layout, "Public profile", PublicProfilePermissions);
                LoginPermissionButton(layout, "Email", EmailPermissions);
                LoginPermissionButton(layout, "User friends", UserFriendsPermissions);
                layout.CreateMargin();

                layout.CreateLabel("Ask for permissions");
                AskForPermissionButton(layout, "Public profile", PublicProfilePermissions);
                AskForPermissionButton(layout, "Email", EmailPermissions);
                AskForPermissionButton(layout, "User friends", UserFriendsPermissions);
                layout.CreateMargin();
                
                layout.CreateLabel("Api Version");
                layout.CreateTextInput(_facebook.ApiVersion, value => {
                    _facebook.ApiVersion = value;
                });
                layout.CreateMargin();

                layout.CreateLabel("Facebook App Id");
                layout.CreateTextInput(_facebook.AppId, value => {
                    _facebook.AppId = value;
                });
            }
        }

        public sealed class AdminPanelFacebookFriends : IAdminPanelGUI
        {
            readonly IFacebook _facebook;

            public AdminPanelFacebookFriends(IFacebook facebook)
            {
                _facebook = facebook;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Facebook Friends");
                layout.CreateMargin();

                if(_facebook.Friends.Count == 0)
                {
                    layout.CreateLabel("No friends to show");    
                }
                else
                {
                    var builder = new StringBuilder();
                    for(int i = 0; i < _facebook.Friends.Count ; ++i)
                    {
                        var friend = _facebook.Friends[i];

                        builder.AppendFormat("{0}.", i).AppendLine()
                            .Append("Name: ").AppendLine(friend.Name)
                            .Append("Id: ").AppendLine(friend.UserId)
                            .AppendFormat("Uses App: {0}", friend.UsesApp).AppendLine()
                            .Append("Profile Image: ").AppendLine(friend.PhotoUrl);

                        layout.CreateTextArea(builder.ToString());
                        layout.CreateMargin();
                        builder.Length = 0;
                    }
                }
            }
        }
    }
}
