using System;
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
        //IDeviceInfo _deviceInfo;
        ILogin _login;
        Text _messagesText;

        public AdminPanelMessageCenter(IMessageCenter messageCenter, IDeviceInfo deviceInfo, ILogin login)
        {
            _mesageCenter = messageCenter;
            //_deviceInfo = deviceInfo;
            _login = login;
        }

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Message Center");
            layout.CreateButton("Load", _mesageCenter.Load);
            layout.CreateLabel("Messages");
            _messagesText = layout.CreateTextArea(MessagesAsText());
            layout.CreateButton("Delete all Messages", DeleteAllMessages, _mesageCenter.Messages.MoveNext());
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
            var result = "";
            var iterator = _mesageCenter.Messages;
            while(iterator.MoveNext())
            {
                result += "\n" + iterator.Current;
            }
            return result;
        }

        void MessageCenterUpdated(IMessageCenter obj)
        {
            _messagesText.text = MessagesAsText();
        }

        void SendTestMessageItself()
        {
            var message = new Message("test", new AttrDic(), new Origin("test name", "test icon"), _login.UserId.ToString()); 
            _mesageCenter.SendMessage(message);
        }

        void DeleteAllMessages()
        {
            var iterator = _mesageCenter.Messages;
            iterator.Reset();
            while(iterator.MoveNext())
            {
                _mesageCenter.DeleteMessage(iterator.Current);
            }
        }
    }
}

