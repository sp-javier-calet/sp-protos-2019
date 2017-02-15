using System;
using System.Text;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using UnityEngine.UI;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkChat : IAdminPanelGUI
    {
        readonly ChatManager _chat;

        public AdminPanelSocialFrameworkChat(ChatManager chat)
        {
            _chat = chat;
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