using UnityEngine;
using System.Collections;
using SocialPoint.AdminPanel;

public class AdminPanelTestConnection : MonoBehaviour {

    public AdminPanel AdminPanel;

	// Use this for initialization
    void Start () {
        AdminPanel.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());
        AdminPanel.AddPanelGUI("Simple", new AdminPanelTestConnectionSimpleGUI());

        AdminPanel.AddPanelGUI("Advanced", new AdminPanelTestConnectionGUI());

        AdminPanel.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());
        AdminPanel.AddPanelGUI("Combined", new AdminPanelTestConnectionSimpleGUI());
        AdminPanel.AddPanelGUI("Combined", new AdminPanelTestConnectionGUI());
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
            AdminPanel.Console.Print("Test log");
        }
    }


    private class AdminPanelTestConnectionGUI : AdminPanelGUI
    {
        public override void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateButton("Test Connection", () => { 
                AdminPanel.Console.Print("Test Connection");
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
