using System;
using System.Collections.Generic;
using System.Text;

using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using UnityEngine.UI;

namespace SocialPoint.ServerMessaging
{
    public class AdminPanelMessageCenter : IAdminPanelGUI, IAdminPanelConfigurer
    {
        IMessageCenter _mesageCenter;
        ILogin _login;
        AdminPanelLayout _layout;

        public AdminPanelMessageCenter(IMessageCenter messageCenter, ILogin login)
        {
            _mesageCenter = messageCenter;
            _login = login;
        }

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;
            layout.CreateLabel("Message Center");
            layout.CreateButton("Load", _mesageCenter.Load);
            layout.CreateLabel("Messages");
            var messages = _mesageCenter.Messages;
            messages.Reset();
            while(messages.MoveNext())
            {
                var hlayout = layout.CreateHorizontalLayout();
                hlayout.CreateTextArea(messages.Current.ToString());
                hlayout.CreateButton("Delete", () => _mesageCenter.DeleteMessage(messages.Current));
            }
            layout.CreateButton("Refresh messages", () => MessageCenterUpdated(_mesageCenter));
            layout.CreateButton("Delete all Messages together", DeleteAllMessagesTogether, _mesageCenter.Messages.MoveNext());
            layout.CreateButton("Delete all Messages one by one", DeleteAllMessagesOneByOne, _mesageCenter.Messages.MoveNext());
            layout.CreateButton("Send Test Message Itself", SendTestMessageItself);
            _mesageCenter.UpdatedEvent += MessageCenterUpdated;
        }

        #endregion

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(SocialPoint.AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Message Center", this));
        }

        #endregion

        string MessagesAsText()
        {
            var iterator = _mesageCenter.Messages;
            var stringBuilder = new StringBuilder();
            while(iterator.MoveNext())
            {
                stringBuilder.AppendFormat("\n{0}",iterator.Current);
            }
            return stringBuilder.ToString();
        }

        void MessageCenterUpdated(IMessageCenter obj)
        {
            if(_layout != null && _layout.IsActiveInHierarchy)
            {
                _layout.Refresh();
            }
            else
            {
                _layout = null;//Clear previous reference
            }
        }

        void SendTestMessageItself()
        {
            var message = new Message("test", new AttrDic(), new Origin("test name", "test icon"), _login.UserId.ToString()); 
            _mesageCenter.SendMessage(message);
        }

        void DeleteAllMessagesOneByOne()
        {
            var iterator = _mesageCenter.Messages;
            iterator.Reset();
            while(iterator.MoveNext())
            {
                _mesageCenter.DeleteMessage(iterator.Current);
            }
        }

        void DeleteAllMessagesTogether()
        {
            var iterator = _mesageCenter.Messages;
            iterator.Reset();

            var list = new List<Message>();
            while(iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }
            _mesageCenter.DeleteMessages(list);
        }
    }
}

