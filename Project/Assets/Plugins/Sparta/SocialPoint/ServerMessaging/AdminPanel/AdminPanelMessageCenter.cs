#if ADMIN_PANEL 

using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.Login;

namespace SocialPoint.ServerMessaging
{
    public sealed class AdminPanelMessageCenter : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly IMessageCenter _messageCenter;
        ILoginData _loginData;

        public AdminPanelMessageCenter(IMessageCenter messageCenter, ILoginData loginData)
        {
            _messageCenter = messageCenter;
            _loginData = loginData;
        }

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Message Center");
            layout.CreateButton("Update messages", () => _messageCenter.UpdateMessages());
            layout.CreateLabel("Messages");
            var messages = _messageCenter.Messages;
            messages.Reset();
            while(messages.MoveNext())
            {
                var hlayout = layout.CreateHorizontalLayout();
                hlayout.CreateTextArea(messages.Current.ToString());
                hlayout.CreateButton("Delete", () => _messageCenter.DeleteMessage(messages.Current));
            }
            messages.Dispose();
            layout.CreateButton("Delete all Messages together", DeleteAllMessagesTogether, _messageCenter.Messages.MoveNext());
            layout.CreateButton("Delete all Messages one by one", DeleteAllMessagesOneByOne, _messageCenter.Messages.MoveNext());
            layout.CreateButton("Send Test Message Itself", SendTestMessageItself);
            layout.CreateButton("Read all Messages together", ReadAllMessagesTogether, _messageCenter.Messages.MoveNext());

            _messageCenter.UpdatedEvent += a => layout.Refresh();
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
            var iterator = _messageCenter.Messages;
            var stringBuilder = new StringBuilder();
            while(iterator.MoveNext())
            {
                stringBuilder.AppendFormat("\n{0}", iterator.Current);
            }
            iterator.Dispose();

            return stringBuilder.ToString();
        }

        void SendTestMessageItself()
        {
            var message = new Message("test", new AttrDic(), new Origin("test name", "test icon"), _loginData.UserId.ToString()); 
            _messageCenter.SendMessage(message);
        }

        void DeleteAllMessagesOneByOne()
        {
            var iterator = _messageCenter.Messages;
            iterator.Reset();

            var list = new List<Message>();
            while(iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }
            iterator.Dispose();

            var listIterator = list.GetEnumerator();
            while(listIterator.MoveNext())
            {
                _messageCenter.DeleteMessage(listIterator.Current);
            }
            listIterator.Dispose();
        }

        void DeleteAllMessagesTogether()
        {
            var iterator = _messageCenter.Messages;
            iterator.Reset();

            var list = new List<Message>();
            while(iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }
            iterator.Dispose();

            _messageCenter.DeleteMessages(list);
        }

        void ReadAllMessagesTogether()
        {
            var iterator = _messageCenter.Messages;
            iterator.Reset();

            var list = new List<Message>();
            while(iterator.MoveNext())
            {
                list.Add(iterator.Current);
            }
            iterator.Dispose();

            _messageCenter.ReadMessages(list);
        }
    }
}

#endif
