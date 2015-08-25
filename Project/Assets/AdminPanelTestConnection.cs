using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

public class AdminPanelTestConnection : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler obj) => 
        {
            obj.AddPanelGUI("System", new AdminPanelTestConnectionGUI());
        };
	}

    private class AdminPanelTestConnectionGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            AdminPanelGUIUtils.CreateButton(layout, "Test Connection", () => { });
        }
    }
}
