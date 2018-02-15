#if ADMIN_PANEL 

using System;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkMessagingSystem : IAdminPanelGUI
    {
        readonly AdminPanelMessagesList _messagesListPanel;
        readonly AdminPanelMessageInfo _messageInfoPanel;
        readonly AdminPanelSendMessage _messageSendPanel;

        public AdminPanelSocialFrameworkMessagingSystem(AdminPanelConsole console, MessagingSystemManager messagesManager)
        {
            _messageInfoPanel = new AdminPanelMessageInfo(messagesManager, console);
            _messagesListPanel = new AdminPanelMessagesList(messagesManager, console, _messageInfoPanel);
            _messageSendPanel = new AdminPanelSendMessage(messagesManager, console);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Messaging System");
            layout.CreateMargin();
            layout.CreateOpenPanelButton("Messages List", _messagesListPanel);
            layout.CreateOpenPanelButton("New Message", _messageSendPanel);
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

            protected void CreateRequestInProgressGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Refreshing...");
            }

            protected void CreateRequestErrorGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Request failed:");
                layout.CreateTextArea(_wampRequestError.ToString());
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

            void CreateMessageGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Id: " + Msg.Id);
                layout.CreateLabel("Origin");
                layout.CreateTextArea(Msg.Origin<IMessageOrigin>().ToString());
                layout.CreateLabel("Payload");
                layout.CreateTextArea(Msg.Payload<IMessagePayload>().ToString());
                CreatePropertiesGUI(layout);
                CreateAddPropertyGUI(layout);
                layout.CreateMargin();

                CreateDeleteMessageGUI(layout);
            }

            void CreatePropertiesGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Properties");

                using(var itr = Msg.GetProperties())
                {
                    while(itr.MoveNext())
                    {
                        var hLayout = layout.CreateHorizontalLayout();
                        hLayout.CreateLabel(itr.Current);
                        MessagingSystemManager.FinishCallback finishCallback = (error, dic) => {
                            _requestInProgress = false;
                            _wampRequestError = error;
                            if(Error.IsNullOrEmpty(_wampRequestError))
                            {
                                _console.Print(string.Format("Deleted property {0} of message {1}", itr.Current, Msg.Id));
                            }
                            else
                            {
                                _console.Print(string.Format("Error deleting property {0} of message {1}. Error: {2}", itr.Current, Msg.Id, _wampRequestError));
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
            }

            void CreateAddPropertyGUI(AdminPanelLayout layout)
            {
                layout.CreateTextInput("Insert new property", property => {
                    MessagingSystemManager.FinishCallback finishCallback = (error, dic) => {
                        _requestInProgress = false;
                        _wampRequestError = error;
                        if(Error.IsNullOrEmpty(_wampRequestError))
                        {
                            _console.Print(string.Format("Added property {0} to message {1}", property, Msg.Id));
                        }
                        else
                        {
                            _console.Print(string.Format("Error adding property {0} to message {1}. Error: {2}", property, Msg.Id, _wampRequestError));
                        }
                        layout.Refresh();
                    };

                    Cancel();
                    _requestInProgress = true;
                    _console.Print(string.Format("Adding property {0} to message {1}", property, Msg.Id));
                    _wampRequest = _messagesManager.AddMessageProperty(property, Msg, finishCallback);
                });
            }

            void CreateDeleteMessageGUI(AdminPanelLayout layout)
            {
                MessagingSystemManager.FinishCallback finishCbk = (error, dic) => {
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

        class AdminPanelSendMessage : BaseMessagingSystemPanel
        {
            string _currentPayload;
            readonly Dictionary<string, AdminPanelEditPayloadPanel> _payloadPanels;
            string _currentDestination;
            readonly Dictionary<string, AdminPanelEditDestinationPanel> _destinationPanels;

            public AdminPanelSendMessage(MessagingSystemManager messagesManager, AdminPanelConsole console) : base(messagesManager, console)
            {
                _payloadPanels = new Dictionary<string, AdminPanelEditPayloadPanel>();
                _payloadPanels.Add(MessagePayloadPlainText.IdentifierKey, new AdminPanelMessagePayloadPlainText());
                _currentPayload = _payloadPanels.Keys.First();

                _destinationPanels = new Dictionary<string, AdminPanelEditDestinationPanel>();
                _destinationPanels.Add("user", new AdminPanelUserDestination());
                _destinationPanels.Add("alliance", new AdminPanelAllianceDestination());
                _currentDestination = _destinationPanels.Keys.First();
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
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
                    layout.CreateLabel("Send Message");
                    layout.CreateMargin();
                    CreateDestinationGUI(layout);
                    layout.CreateMargin();
                    CreatePayloadGUI(layout);
                    CreateSendGUI(layout);
                }
            }

            void CreateDestinationGUI(AdminPanelLayout layout)
            {
                var fLayout = layout.CreateFoldoutLayout("Destination type:");
                using(var itr = _destinationPanels.GetEnumerator())
                {
                    while(itr.MoveNext())
                    {
                        var currentStr = itr.Current.Key;
                        bool status = currentStr == _currentDestination;
                        fLayout.CreateToggleButton(currentStr, status, selected => {
                            _currentDestination = currentStr;
                            layout.Refresh();
                        });
                    }
                }

                layout.CreateTextArea(_destinationPanels[_currentDestination].GetData().ToString());
                layout.CreateOpenPanelButton("Edit destination", _destinationPanels[_currentDestination]);
            }

            void CreatePayloadGUI(AdminPanelLayout layout)
            {
                var fLayout = layout.CreateFoldoutLayout("Payload type:");
                using(var itr = _payloadPanels.GetEnumerator())
                {
                    while(itr.MoveNext())
                    {
                        var currentStr = itr.Current.Key;
                        bool status = currentStr == _currentPayload;
                        fLayout.CreateToggleButton(currentStr, status, selected => {
                            _currentPayload = currentStr;
                            layout.Refresh();
                        });
                    }
                }

                layout.CreateTextArea(_payloadPanels[_currentPayload].GetPayload().ToString());
                layout.CreateOpenPanelButton("Edit payload", _payloadPanels[_currentPayload]);
            }

            void CreateSendGUI(AdminPanelLayout layout)
            {
                layout.CreateButton("Send", () => {
                    var payload = _payloadPanels[_currentPayload].GetPayload();
                    var destination = _destinationPanels[_currentDestination].GetData();
                    MessagingSystemManager.FinishCallback finishCallback = (error, dic) => {
                        _requestInProgress = false;
                        _wampRequestError = error;
                        if(Error.IsNullOrEmpty(_wampRequestError))
                        {
                            _console.Print(string.Format("Message with payload {0} sent to {1}", payload, destination));
                        }
                        else
                        {
                            _console.Print(string.Format("Error sending message {0} to {1}. Error: {2}", payload, destination, _wampRequestError));
                        }
                        layout.Refresh();
                    };
                    Cancel();
                    _requestInProgress = true;
                    _console.Print(string.Format("Sending message {0} to {1}", payload, destination));
                    _messagesManager.SendMessage(destination.Type, destination.Data, payload, finishCallback);
                });
            }
        }

        public class DestinationData
        {
            public string Type{ get; set; }

            public AttrDic Data{ get; set; }

            public DestinationData()
            {
                Data = new AttrDic();
            }

            public override string ToString()
            {
                return string.Format("[DestinationData: Type={0}, Data={1}]", Type, Data);
            }
        }

        public abstract class AdminPanelEditDestinationPanel : IAdminPanelGUI
        {
            public abstract DestinationData GetData();

            public abstract void OnCreateGUI(AdminPanelLayout layout);
        }

        class AdminPanelUserDestination : AdminPanelEditDestinationPanel
        {
            public DestinationData Destination{ get; private set; }

            public AdminPanelUserDestination()
            {
                Destination = new DestinationData();
                Destination.Type = "user";
                Destination.Data.SetValue("id", 0L);
            }

            public override DestinationData GetData()
            {
                return Destination;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Edit user destination");
                layout.CreateMargin();
                var idInput = layout.CreateTextInput("UserId", id => Destination.Data.SetValue("id", Int64.Parse(id)));
                idInput.text = Destination.Data.GetValue("id").ToString();

                layout.CreateButton("Ok", layout.ClosePanel);
            }
        }

        class AdminPanelAllianceDestination : AdminPanelEditDestinationPanel
        {
            public DestinationData Destination{ get; private set; }

            public AdminPanelAllianceDestination()
            {
                Destination = new DestinationData();
                Destination.Type = "alliance";
                Destination.Data.SetValue("id", "");
            }

            public override DestinationData GetData()
            {
                return Destination;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Edit alliance destination");
                layout.CreateMargin();
                var idInput = layout.CreateTextInput("AllianceId", id => Destination.Data.SetValue("id", id));
                idInput.text = Destination.Data.GetValue("id").ToString();

                layout.CreateButton("Ok", layout.ClosePanel);
            }
        }

        public abstract class AdminPanelEditPayloadPanel : IAdminPanelGUI
        {
            public abstract IMessagePayload GetPayload();

            public abstract void OnCreateGUI(AdminPanelLayout layout);
        }

        class AdminPanelMessagePayloadPlainText : AdminPanelEditPayloadPanel
        {
            string _title;
            string _text;

            public override IMessagePayload GetPayload()
            {
                return new MessagePayloadPlainText(_title, _text);
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Edit plain text payload");
                layout.CreateMargin();
                var titleInput = layout.CreateTextInput("Title", title => {
                    _title = title;
                });
                titleInput.text = _title;
                var textInput = layout.CreateTextInput("Text", text => {
                    _text = text;
                });
                textInput.text = _text;
                layout.CreateButton("Ok", layout.ClosePanel);
            }
        }
    }
}

#endif
