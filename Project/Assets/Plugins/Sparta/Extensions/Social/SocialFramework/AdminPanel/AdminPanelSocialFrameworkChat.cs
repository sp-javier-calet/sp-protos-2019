#if ADMIN_PANEL 

using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkChat : IAdminPanelGUI
    {
        readonly ChatManager _chat;
        readonly AdminPanelConsole _console;

        public AdminPanelSocialFrameworkChat(ChatManager chat, AdminPanelConsole console)
        {
            _chat = chat;
            _console = console;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Chat");
            layout.CreateMargin();

            var itr = _chat.GetRooms();
            while(itr.MoveNext())
            {
                var room = itr.Current;
                layout.CreateOpenPanelButton(room.Type, new AdminPanelChatRoom(room), room.Subscribed);
            }
            itr.Dispose();

            layout.CreateMargin();
            layout.CreateLabel("Ban info:");
            layout.CreateLabel(GetBanInfo());
            layout.CreateMargin();

            layout.CreateMargin();
            layout.CreateLabel("Reporting");
            layout.CreateMargin();
            {
                var hLayout = layout.CreateHorizontalLayout();
                hLayout.CreateFormLabel("Report User:");
                hLayout.CreateTextInput("Insert reported User ID", insertedText => {
                    if(!_chat.CanReportUser(insertedText))
                    {
                        _console.Print(string.Format("You cannot report more times the user {0}", insertedText));
                        return;
                    }

                    var fakeMessage = new BaseChatMessage();
                    fakeMessage.Text = "Fake text with a lot of swearing";
                    fakeMessage.Uuid = "2f68cf8f-039a-4308-95d2-fc430ac5ebbb";
                    fakeMessage.MessageData = new MessageData();
                    fakeMessage.MessageData.PlayerId = insertedText;

                    var extraData = new AttrDic();
                    extraData.SetValue("extra_variable", "extra_value");
                    _chat.ReportChatMessage(fakeMessage, extraData);

                    layout.Refresh();
                });
            }
            layout.CreateMargin();
            {
                layout.CreateLabel(string.Format("Current report cooldown {0}", _chat.ReportUserCooldown));
                var hLayout = layout.CreateHorizontalLayout();
                hLayout.CreateFormLabel("Change report cooldown");
                hLayout.CreateTextInput("Insert new cooldown", (insertedText) => {
                    int newCooldown;
                    if(!Int32.TryParse(insertedText, out newCooldown))
                    {
                        _console.Print(string.Format("Invalid cooldown value: {0}", insertedText));
                        return;
                    }
                    _chat.ReportUserCooldown = newCooldown;

                    layout.Refresh();
                });
            }

            layout.CreateLabel("Current Reports");
            var builder = StringUtils.StartBuilder();
            var itrReports = _chat.Reports.GetEnumerator();
            while(itrReports.MoveNext())
            {
                builder.AppendLine(itrReports.Current.ToString());
            }
            itr.Dispose();

            layout.CreateVerticalScrollLayout().CreateTextArea(builder.ToString());
            layout.CreateMargin();
        }

        string GetBanInfo()
        {
            return _chat.ChatBanEndTimestamp > 0 ? 
                string.Format("You are banned until {0}", _chat.ChatBanEndTimestamp) : 
                "You are a legal player!";
        }


        #region Chat Inner classes

        class AdminPanelChatRoom : IAdminPanelManagedGUI
        {
            public string Name
            {
                get
                {
                    return _room.Type;
                }
            }

            Text _text;
            readonly StringBuilder _content;
            readonly IChatRoom _room;

            public AdminPanelChatRoom(IChatRoom room)
            {
                _room = room;
                _content = new StringBuilder();
            }

            public void OnOpened()
            {
                _room.Messages.OnMessageAdded += OnMessageListChanged;
                _room.Messages.OnMessageEdited += OnMessageListChanged;
            }

            public void OnClosed()
            {
                _room.Messages.OnMessageAdded -= OnMessageListChanged;
                _room.Messages.OnMessageEdited -= OnMessageListChanged;
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel(Name);
                layout.CreateMargin();

                RefreshChatContent();

                _text = layout.CreateTextArea(_content.ToString());
                layout.CreateTextInput(_room.SendDebugMessage);
            }

            void OnMessageListChanged(int idx)
            {
                RefreshChatContent();
            }

            void RefreshChatContent()
            {
                _content.Length = 0;

                _room.Messages.ProcessMessages((idx, msg) => {
                    if(msg.IsWarning)
                    {
                        _content.AppendLine(msg.Text);
                    }
                    else
                    {
                        if(msg.IsSending)
                        {
                            _content.Append(">>");
                        }

                        var allianceText = string.Empty;
                        var data = msg.MessageData;
                        if(data.HasAlliance)
                        {
                            allianceText = string.Format("[{0}({1})]", data.AllianceName, data.AllianceId);
                        }
                        _content.AppendFormat("{0}({1}) {2}: {3}", data.PlayerName, data.PlayerId, allianceText, msg.Text).AppendLine();
                    }
                });

                if(_text != null)
                {
                    _text.text = _content.ToString();
                }
            }
        }

        #endregion
    }
}

#endif
