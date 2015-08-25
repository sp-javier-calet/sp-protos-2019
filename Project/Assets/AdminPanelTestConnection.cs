using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

public class AdminPanelTestConnection : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
        {
            handler.AddPanelGUI("System", new AdminPanelTestConnectionGUI());

            handler.AddPanelGUI("Game", new AdminPanelTestConnectionGUI());

            handler.AddPanelGUI("Backend", new AdminPanelTestConnectionGUI());
        };
	}

    private class AdminPanelTestConnectionGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            AdminPanelGUIUtils.CreateButton(layout, "Test Connection", () => { 
                Debug.Log("Test Connection");
            });
        }
    }
}
