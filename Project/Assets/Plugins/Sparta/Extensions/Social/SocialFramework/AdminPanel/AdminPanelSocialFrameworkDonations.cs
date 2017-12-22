#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using System;
using UnityEngine.UI;

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
            readonly long _localUserId;

            int _itemId;
            int _amount;
            string _donationType;
            AttrDic _metadata;

            Text _textArea;

            public AdminPanelSendRequest(DonationsManager manager, AdminPanelConsole console, long localUserId) : base(manager, console)
            {
                _localUserId = localUserId;
                _metadata = new AttrDic();
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                CreateSendGUI(layout);
            }

            void CreateSendGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Request Donation Data");
                layout.CreateMargin();

                layout.CreateLabel("Requester Id: " + _localUserId);
                
                layout.CreateTextInput("ItemId", text => {
                    int itemId;
                    TryParseInt(text, out itemId);

                    if(itemId > 0)
                    {
                        _itemId = itemId;
                    }
                    else
                    {
                        _console.Print("ItemId must be greater than zero");
                    }
                    RefreshDonationInfo();
                });

                layout.CreateTextInput("Amount", text => {
                    int itemId;
                    TryParseInt(text, out itemId);

                    if(itemId > 0)
                    {
                        _amount = itemId;
                    }
                    else
                    {
                        _console.Print("Amount must be greater than zero");
                    }
                    RefreshDonationInfo();
                });

                layout.CreateTextInput("Donation Type", text => {
                    _donationType = text;
                    RefreshDonationInfo();
                });

                const string infoText = " RequesterId: {0}\\n ItemId: {1}\\n Amount: {2}\\n Type: {3}\\n Metadata: {4}";
                _textArea = layout.CreateTextArea(string.Format(infoText, _localUserId, _itemId, _amount, _donationType, _metadata));

                layout.CreateButton("Send", () => {
                    Action<Error, ItemRequest> finishCallback = (err, itemRequest) => {
                        _requestInProgress = false;
                        _wampRequestError = err;

                        string info;
                        if(Error.IsNullOrEmpty(err))
                        {
                            info = "Request {0} with itemId {1} and amount {2} sent";
                            info = string.Format(info, itemRequest.RequestUuid, itemRequest.ItemId, itemRequest.Amount);
                        }
                        else
                        {
                            info = "Error sending request";
                        }
                        _console.Print(info);
                        layout.Refresh();
                    };

                    _manager.RequestItem(_itemId, _amount, _donationType, _metadata, finishCallback);
                    _requestInProgress = true;
                });
            }

            void RefreshDonationInfo()
            {
                const string infoText = " RequesterId: {0}\\n ItemId: {1}\\n Amount: {2}\\n Type: {3}\\n Metadata: {4}";
                _textArea.text = string.Format(infoText, _localUserId, _itemId, _amount, _donationType, _metadata);
            }

            bool TryParseInt(string text, out int num)
            {
                try
                {
                    num = Int32.Parse(text);
                }
                catch(Exception e)
                {
                    _console.Print(e.ToString());
                    num = 0;
                    return false;
                }

                return true;
            }
        }

        class AdminPanelItemRequest : BaseDonationsPanel
        {
            AdminPanelLayout _layout;
            readonly long _localUserId;

            public ItemRequest ItemRequest{ get; set; }

            public AdminPanelItemRequest(DonationsManager manager, AdminPanelConsole console, long localUserId) : base(manager, console)
            {
                _localUserId = localUserId;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                _layout = layout;
                CreateItemRequestGUI(layout);
            }

            public override void OnOpened()
            {
                base.OnOpened();
                _manager.DonationsSignal += OnDonationSignalReceived;
            }

            public override void OnClosed()
            {
                base.OnClosed();
                _manager.DonationsSignal -= OnDonationSignalReceived;
            }

            void OnDonationSignalReceived(DonationsManager.ActionType action, AttrDic dict)
            {
                _console.Print("DonationsSignal received");
                _layout.Refresh();
            }

            void CreateItemRequestGUI(AdminPanelLayout layout)
            {
                if(ItemRequest == null)
                {
                    _console.Print("Item Request is null, closing panel.");
                    layout.ClosePanel();
                    return;
                }

                bool isMyRequest = _localUserId == ItemRequest.RequesterId;

                Action<Error> collectCallback = err =>
                {
                    _requestInProgress = false;
                    _wampRequestError = err;
                    string result = (Error.IsNullOrEmpty(err) ? "true" : "false");
                    _console.Print("Collect Action Success: " + result);
                    layout.Refresh();
                };

                layout.CreateLabel("Item Request");
                layout.CreateMargin();

                layout.CreateTextArea(ItemRequest.ToStringExtended());

                layout.CreateLabel("Contributions");

                bool hasPendingCollects = false;
                using(var itr = ItemRequest.ReceivedMapEnumerator)
                {
                    while(itr.MoveNext())
                    {
                        var contributorId = itr.Current.Key;
                        var amountReceived = itr.Current.Value;

                        var amountCollected = ItemRequest.GetCollectedBy(contributorId);

                        bool isPendingCollect = amountReceived > amountCollected;

                        hasPendingCollects |= isPendingCollect;

                        const string btnMessage = "ContributorId: {0}\n AmountReceived: {1} - AmountCollected: {2}";
                        var hlayout = layout.CreateHorizontalLayout();
                        hlayout.CreateLabel(string.Format(btnMessage, contributorId, amountReceived, amountCollected));

                        if(isMyRequest && isPendingCollect)
                        {
                            hlayout.CreateButton("Collect", () =>{
                                _requestInProgress = true;
                                _manager.CollectItem(contributorId, ItemRequest.RequestUuid, ItemRequest.DonationType, collectCallback);
                            });
                        }
                    }
                }
            }
        }

        #endregion

        readonly AdminPanelSendRequest _requestSendPanel;
        readonly AdminPanelItemRequest _requestPanel;
        readonly long _localUserId;

        AdminPanelLayout _layout;

        public AdminPanelSocialFrameworkDonations(AdminPanelConsole console, DonationsManager manager, long localUserId) : base(manager, console)
        {
            _localUserId = localUserId;

            _requestSendPanel = new AdminPanelSendRequest(manager, console, localUserId);
            _requestPanel = new AdminPanelItemRequest(manager, console, localUserId);
        }

        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;
            layout.CreateLabel("Donations");
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

        public override void OnOpened()
        {
            base.OnOpened();
            _manager.DonationsSignal += OnDonationSignalReceived;
        }

        public override void OnClosed()
        {
            base.OnClosed();
            _manager.DonationsSignal -= OnDonationSignalReceived;
        }

        void OnDonationSignalReceived(DonationsManager.ActionType action, AttrDic dict)
        {
            _console.Print("DonationsSignal received");
            _layout.Refresh();
        }

        void CreateDonationsGUI(AdminPanelLayout layout)
        {
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
