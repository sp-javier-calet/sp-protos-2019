using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

public class AdminPanelTestConnection : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    AdminPanelHandler.OnAdminPanelInit += (AdminPanelHandler handler) => 
        {
            handler.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());
            handler.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());

            handler.AddPanelGUI("Advanced", new AdminPanelTestConnectionGUI());

            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());
            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionSimpleGUI());
            handler.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());

        };
	}

    private class AdminPanelTestConnectionSimpleGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel( "TestLabel");
            layout.CreateButton("TestButton", TestAction);

            using(var horizontalLayout = layout.CreateHorizontalLayout())
            {
                horizontalLayout.CreateLabel("HLabel1");
                horizontalLayout.CreateLabel("HLabel2");
            }

            layout.CreateToggleButton("TestToggle", false, (value) => {});

            using(var horizontalLayout = layout.CreateHorizontalLayout())
            {
                horizontalLayout.CreateToggleButton("TestToggle", false, (value) => {});
                horizontalLayout.CreateButton("TestButton", TestAction);
            }
        }

        private void TestAction()
        {
            Console.Print("Test log");
        }
    }


    private class AdminPanelTestConnectionGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateButton("Test Connection", () => { 
                Debug.Log("Test Connection");
            });

            using(var scrollLayout = layout.CreateVerticalScrollLayout())
            {
                scrollLayout.CreateButton("TestButton1", () => {});
                scrollLayout.CreateLabel("TestLabel");
                scrollLayout.CreateButton("testButton2", () => {});


                using(var horizontalLayout = scrollLayout.CreateHorizontalLayout())
                {
                    horizontalLayout.CreateButton("LabeledButton1", () => {});
                    horizontalLayout.CreateButton("LabeledButton2", () => {});
                    horizontalLayout.CreateButton("LabeledButton3", () => {});
                    horizontalLayout.CreateButton("LabeledButton4", () => {});
                }

                using(var horizontalLayout = scrollLayout.CreateHorizontalLayout())
                {
                    horizontalLayout.CreateLabel("HLabel1");
                    horizontalLayout.CreateLabel("HLabel2");
                }
                
                using(var horizontalLayout = scrollLayout.CreateHorizontalLayout())
                {
                    horizontalLayout.CreateToggleButton("LabeledToogle1", false, (value) => {});
                    horizontalLayout.CreateToggleButton("LabeledToogle2", true, (value) => {});
                }

                using(var horizontalLayout = scrollLayout.CreateHorizontalLayout())
                {
                    horizontalLayout.CreateButton("LabeledButton1", () => {});
                    horizontalLayout.CreateButton("LabeledButton2", () => {});
                    horizontalLayout.CreateButton("LabeledButton3", () => {});
                }

                using(var horizontalLayout = scrollLayout.CreateHorizontalLayout())
                {
                    horizontalLayout.CreateLabel("HLabel1");
                    horizontalLayout.CreateLabel("HLabel2");
                }
            }
        }
    }
}
