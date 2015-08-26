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

            using(var scrollLayout = new VerticalScrollLayout(layout))
            {
                AdminPanelGUIUtils.CreateButton(scrollLayout, "testButton1", () => {});
                AdminPanelGUIUtils.CreateButton(scrollLayout, "testButton2", () => {});
                AdminPanelGUIUtils.CreateButton(scrollLayout, "testButton3", () => {});
                AdminPanelGUIUtils.CreateMargin(scrollLayout);
                AdminPanelGUIUtils.CreateLabel(scrollLayout, "TestLabel");
                AdminPanelGUIUtils.CreateButton(scrollLayout, "testButton4", () => {});

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1");
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2");
                }
            }
        }
    }
}
