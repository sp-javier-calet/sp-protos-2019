using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using SocialPoint.AdminPanel;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelSceneSelector : IAdminPanelConfigurer, IAdminPanelGUI
    {
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Scene Selector", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            //List of scenes in build
            layout.CreateLabel("Select Scene");
            string[] scenes = ScenesData.Instance.ScenesNames;
            for(int i = 0; i < scenes.Length; i++)
            {
                var name = scenes[i];
                layout.CreateButton(name, () => {
                    SceneManager.LoadScene(name);
                });
            }

            //Warning message
            layout.CreateMargin();
            var infoLabel = layout.CreateLabel("Warning: Scene changes are not always clean. Remaining data from previous scenes can persist and create errors");
            infoLabel.fontSize /= 2;//Set info text as half the size of default label text
            infoLabel.fontStyle = FontStyle.Italic;
        }
    }
}
