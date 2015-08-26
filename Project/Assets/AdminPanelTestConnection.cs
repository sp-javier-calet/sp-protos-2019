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
                AdminPanelGUIUtils.CreateMargin(scrollLayout);
                AdminPanelGUIUtils.CreateLabel(scrollLayout, "TestLabel");
                AdminPanelGUIUtils.CreateButton(scrollLayout, "testButton2", () => {});


                using(var horizontalLayout = new HorizontalLayout(scrollLayout, new Vector2(1.0f, 0.0f)))
                {
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton1", () => {}, new Vector2(1.0f/4, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton2", () => {}, new Vector2(1.0f/4, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton3", () => {}, new Vector2(1.0f/4, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton4", () => {});
                    horizontalLayout.AdjustMinHeight();
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1", new Vector2(0.5f, 1.0f));
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2", new Vector2(0.5f, 1.0f));
                    horizontalLayout.AdjustMinHeight();
                }
                
                using(var horizontalLayout = new HorizontalLayout(scrollLayout, new Vector2(1.0f, 0.0f)))
                {
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton1", () => {}, new Vector2(1.0f/3, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton2", () => {});
                    horizontalLayout.AdjustMinHeight();
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton1", () => {}, new Vector2(1.0f/3, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton2", () => {}, new Vector2(1.0f/3, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton3", () => {});
                    horizontalLayout.AdjustMinHeight();
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1", new Vector2(0.5f, 1.0f));
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2", new Vector2(0.5f, 1.0f));
                    horizontalLayout.AdjustMinHeight();
                }
            }
        }
    }
}
