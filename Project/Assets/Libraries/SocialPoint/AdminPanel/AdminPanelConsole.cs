using System;
using System.Collections;

namespace SocialPoint.AdminPanel
{
    public class AdminPanelConsole {

        public string Content { get; private set; }

        public event Action OnContentChanged;

        public AdminPanelConsole()
        {
            Clear();
        }

        public void Print(string text)
        {
            Content += text + "\n";
            OnContentChanged();
        }

        public void Clear()
        {
            Content = "";
        }
    }
}