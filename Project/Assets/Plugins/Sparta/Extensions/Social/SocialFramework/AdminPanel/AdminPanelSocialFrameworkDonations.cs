#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using System;

namespace SocialPoint.Social
{
    #region Base Panels

    public abstract class BaseDonationsPanel : AdminPanelSocialFramework.BaseRequestPanel
    {
        protected readonly DonationsManager _manager;
        protected readonly AdminPanelConsole _console;
        protected bool _requestInProgress;

        protected BaseDonationsPanel(DonationsManager manager, AdminPanelConsole console)
        {
            _manager = manager;
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
    public class AdminPanelSocialFrameworkDonations : BaseDonationsPanel
    {
        #region Child Panels

        class AdminPanelSendRequest : BaseDonationsPanel
        {
            int _itemId;
            int _amount;
            string _donationType;
            AttrDic _metadata;

            public AdminPanelSendRequest(DonationsManager manager, AdminPanelConsole console) : base(manager, console)
            {
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                throw new System.NotImplementedException();
            }

            void CreateSendGUI(AdminPanelLayout layout)
            {

            }
        }

        class AdminPanelItemRequest : BaseDonationsPanel
        {
            public ItemRequest ItemRequest{ get; set; }

            public AdminPanelItemRequest(DonationsManager manager, AdminPanelConsole console) : base(manager, console)
            {
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion

        readonly AdminPanelSendRequest _requestSendPanel;
        readonly AdminPanelItemRequest _requestPanel;
        readonly long _localUserId;

        public AdminPanelSocialFrameworkDonations(AdminPanelConsole console, DonationsManager manager, long localUserId) : base(manager, console)
        {
            _localUserId = localUserId;

            _requestSendPanel = new AdminPanelSendRequest(manager, console);
            _requestPanel = new AdminPanelItemRequest(manager, console);
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
                CreateDonationsGUI(layout);
            }
        }

        void CreateDonationsGUI(AdminPanelLayout layout)
        {
            _manager.DonationsSignal += (action, dict) => {
                _console.Print("DonationsSignal received");
                layout.Refresh();
            };
            
            layout.CreateLabel("Donations Manager");
            layout.CreateMargin();

            layout.CreateOpenPanelButton("New Donation Request", _requestSendPanel);

            layout.CreateLabel("Requests");

            Action<Error> contributeCallback = (err) => {
                _requestInProgress = false;
                _wampRequestError = err;
                string result = Error.IsNullOrEmpty(err) ? "true" : "false";
                _console.Print("Contribute Action Success: " + result);
                layout.Refresh();
            };

            Action<Error> removeCallback = err => {
                _requestInProgress = false;
                _wampRequestError = err;
                string result = Error.IsNullOrEmpty(err) ? "true" : "false";
                _console.Print("Remove Action Success: " + result);
                layout.Refresh();
            };

            foreach(var request in _manager.ItemsRequests)
            {
                var isMyRequest = _localUserId == request.RequesterId;

                var btnText = request.ToString();
                var hlayout = layout.CreateHorizontalLayout();

                hlayout.CreateButton(btnText, () => {
                    _requestPanel.ItemRequest = request;
                    layout.OpenPanel(_requestPanel);
                });

                if(isMyRequest)
                {
                    hlayout.CreateButton("Remove", () => {
                        _requestInProgress = true;
                        _manager.RemoveRequest(request.RequestUuid, request.DonationType, removeCallback);
                    });
                }
                else
                {
                    hlayout.CreateButton("Contribute", () => {
                        _requestInProgress = true;
                        _manager.ContributeItem(request.RequesterId, request.RequestUuid, 1, request.DonationType, contributeCallback);
                    });
                }
            }
        }
    }
}
#endif
