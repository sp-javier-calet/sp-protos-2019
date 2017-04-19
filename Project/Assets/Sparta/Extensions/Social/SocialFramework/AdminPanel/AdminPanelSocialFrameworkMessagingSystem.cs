#if ADMIN_PANEL 

using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkMessagingSystem : IAdminPanelGUI
    {
        readonly AdminPanelMessagesList _messagesListPanel;
        readonly AdminPanelMessageInfo _messageInfoPanel;

        public AdminPanelSocialFrameworkMessagingSystem(AdminPanelConsole console, MessagingSystemManager messagesManager)
        {
            _messageInfoPanel = new AdminPanelMessageInfo(messagesManager, console);
            _messagesListPanel = new AdminPanelMessagesList(messagesManager, console, _messageInfoPanel);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Messaging System");
            layout.CreateMargin();
            layout.CreateOpenPanelButton("Messages List", _messagesListPanel);
        }

        #region Base Panels

        abstract class BaseMessagingSystemPanel : AdminPanelSocialFramework.BaseRequestPanel
        {
            protected readonly MessagingSystemManager _messagesManager;
            protected readonly AdminPanelConsole _console;
            protected bool _requestInProgress;

            public BaseMessagingSystemPanel(MessagingSystemManager messagesManager, AdminPanelConsole console)
            {
                _messagesManager = messagesManager;
                _console = console;
                _requestInProgress = false;
            }
        }

        #endregion

        class AdminPanelMessagesList : BaseMessagingSystemPanel
        {
            readonly AdminPanelMessageInfo _infoPanel;

            public AdminPanelMessagesList(MessagingSystemManager messagesManager, AdminPanelConsole console, AdminPanelMessageInfo infoPanel) : base(messagesManager, console)
            {
                _infoPanel = infoPanel;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Messages List");
                layout.CreateMargin();

                using(var itr = _messagesManager.GetMessages().GetEnumerator())
                {
                    while(itr.MoveNext())
                    {
                        var message = itr.Current;
                        var messageLabel = message.ToString();
                        layout.CreateButton(messageLabel, () => {
                            _infoPanel.Msg = message;
                            layout.OpenPanel(_infoPanel);
                        });
                    }
                }
                layout.CreateMargin();
            }
        }

        class AdminPanelMessageInfo : BaseMessagingSystemPanel
        {
            public Message Msg { get; set; }

            public AdminPanelMessageInfo(MessagingSystemManager messagesManager, AdminPanelConsole console) : base(messagesManager, console)
            {
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Message Info");
                layout.CreateMargin();

                if(_requestInProgress)
                {
                    CreateRequestInProgressGUI(layout);
                }
                else if(!Error.IsNullOrEmpty(_wampRequestError))
                {
                    CreateRequestErrorGUI(layout);
                }
                else
                {
                    CreateMessageGUI(layout);
                }
            }

            void CreateRequestInProgressGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Refreshing...");
            }

            void CreateRequestErrorGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Request failed:");
                layout.CreateTextArea(_wampRequestError.ToString());
            }

            void CreateMessageGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Id: " + Msg.Id);
                layout.CreateLabel("Origin");
                layout.CreateTextArea(Msg.Origin<IMessageOrigin>().ToString());
                layout.CreateLabel("Payload");
                layout.CreateTextArea(Msg.Payload<IMessagePayload>().ToString());
                layout.CreateLabel("Properties");

                using(var itr = Msg.GetProperties())
                {
                    while(itr.MoveNext())
                    {
                        var hLayout = layout.CreateHorizontalLayout();
                        hLayout.CreateLabel(itr.Current);
                        MessagingSystemManager.FinishCallback finishCallback = (Error error, AttrDic dic) => {
                            _requestInProgress = false;
                            _wampRequestError = error;
                            if(Error.IsNullOrEmpty(_wampRequestError))
                            {
                                _console.Print(string.Format("Deleted property {0} of message {1}", itr.Current, Msg.Id));
                            }
                            else
                            {
                                _console.Print(string.Format("Error deleteing property {0} of message {1}. Error: {2}", itr.Current, Msg.Id, _wampRequestError));
                            }
                            layout.Refresh();
                        };
                        hLayout.CreateButton("Delete", ButtonColor.Red, () => {
                            Cancel();
                            _requestInProgress = true;
                            _console.Print(string.Format("Deleting property {0} of message {1}", itr.Current, Msg.Id));
                            _wampRequest = _messagesManager.RemoveMessageProperty(itr.Current, Msg, finishCallback);
                            layout.Refresh();
                        });
                    }
                }

                layout.CreateMargin();

                MessagingSystemManager.FinishCallback finishCbk = (Error error, AttrDic dic) => {
                    _requestInProgress = false;
                    _wampRequestError = error;
                    if(Error.IsNullOrEmpty(_wampRequestError))
                    {
                        _console.Print(string.Format("Deleted message {0}", Msg.Id));
                        layout.ClosePanel();
                    }
                    else
                    {
                        _console.Print(string.Format("Error deleteing message {0}. Error: {1}", Msg.Id, _wampRequestError));
                        layout.Refresh();
                    }
                };
                layout.CreateConfirmButton("Delete Message", ButtonColor.Red, () => {
                    Cancel();
                    _requestInProgress = true;
                    _console.Print(string.Format("Deleting message {0}", Msg.Id));
                    _wampRequest = _messagesManager.DeleteMessage(Msg, finishCbk);
                    layout.Refresh();
                });
            }
        }
    }
}

#endif
