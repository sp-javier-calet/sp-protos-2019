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
        AdminPanel.AdminPanel _adminPanel;

        public AdminPanelNativeCallsHandler(NativeCallsHandler handler)
        {
            _nativeCallsHandler = handler;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _adminPanel = adminPanel;
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
                _adminPanel.Console.Print(string.Format("registering to {0} native call", item.Key)); 
                item.Value.ArgMethod -= PrintCall;
                item.Value.ArgMethod += PrintCall;
            }
            itr.Dispose();
        }

        void PrintCall(string call)
        {
            _adminPanel.Console.Print(call);   
        }

    }
}

