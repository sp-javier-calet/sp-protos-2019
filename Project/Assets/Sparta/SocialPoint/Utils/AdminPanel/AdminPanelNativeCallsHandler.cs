#if ADMIN_PANEL 

using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using UnityEngine.UI;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelNativeCallsHandler : IAdminPanelGUI, IAdminPanelConfigurer
    {
        NativeCallsHandler _nativeCallsHandler;
        AdminPanelConsole _console;

        public AdminPanelNativeCallsHandler(NativeCallsHandler handler)
        {
            _nativeCallsHandler = handler;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Native Calls Handler", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateButton("Register to all calls", RegisterToAllCalls);
        }

        #endregion

        void RegisterToAllCalls()
        {
            var listeners = Reflection.GetPrivateField<NativeCallsHandler, IDictionary<string, NativeCallsHandler.EventMethodHolder>>(_nativeCallsHandler, "_listeners");
            var itr = listeners.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                ConsolePrint(string.Format("registering to {0} native call", item.Key)); 
                item.Value.ArgMethod -= ConsolePrint;
                item.Value.ArgMethod += ConsolePrint;
            }
            itr.Dispose();
        }

        void ConsolePrint(string msg)
        {
            if(_console != null)
            {
                _console.Print(msg);   
            }
        }

    }
}

#endif
