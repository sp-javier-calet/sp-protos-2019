using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

public class AdminPanelTestConnection : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
        {
            handler.AddPanelGUI("Test", new AdminPanelTestConnectionGUI());

            handler.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());
            handler.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());

            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());
            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionSimpleGUI());
            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());

        };
	}

    private class AdminPanelTestConnectionSimpleGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            AdminPanelGUIUtils.CreateMargin(layout);
            AdminPanelGUIUtils.CreateLabel(layout, "TestLabel");
            AdminPanelGUIUtils.CreateButton(layout, "TestButton", () => {});

            using(var horizontalLayout = new HorizontalLayout(layout))
            {
                AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1", new Vector2(0.5f, 1.0f));
                AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2", new Vector2(0.5f, 1.0f));
            }

            AdminPanelGUIUtils.CreateButton(layout, "TestButton", () => {});
            AdminPanelGUIUtils.CreateButton(layout, "TestButton", () => {});
        }
    }


    private class AdminPanelTestConnectionGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            AdminPanelGUIUtils.CreateButton(layout, "Test Connection", () => { 
                Debug.Log("Test Connection");
            });

            using(var scrollLayout = new VerticalScrollLayout(layout, new Vector2(1.0f, 0.5f)))
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
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1", new Vector2(0.5f, 1.0f));
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2", new Vector2(0.5f, 1.0f));
                }
                
                using(var horizontalLayout = new HorizontalLayout(scrollLayout, new Vector2(1.0f, 0.0f)))
                {
                    AdminPanelGUIUtils.CreateToggleButton(horizontalLayout, "LabeledToogle1", false, (value) => {}, new Vector2(0.5f, 1.0f));
                    AdminPanelGUIUtils.CreateToggleButton(horizontalLayout, "LabeledToogle2", true, (value) => {});
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton1", () => {}, new Vector2(1.0f/3, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton2", () => {}, new Vector2(1.0f/3, 1.0f));
                    AdminPanelGUIUtils.CreateButton(horizontalLayout, "LabeledButton3", () => {});
                }

                using(var horizontalLayout = new HorizontalLayout(scrollLayout))
                {
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel1", new Vector2(0.5f, 1.0f));
                    AdminPanelGUIUtils.CreateLabel(horizontalLayout, "HLabel2", new Vector2(0.5f, 1.0f));
                }

            }
        }
    }
}
