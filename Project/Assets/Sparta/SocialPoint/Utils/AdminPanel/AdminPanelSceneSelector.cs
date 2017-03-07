#if ADMIN_PANEL 

using UnityEngine;
using UnityEngine.SceneManagement;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Dependency;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelSceneSelector : IAdminPanelConfigurer, IAdminPanelGUI
    {
        bool _clearServices;

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
                    Services.Instance.Clear();
                    SceneManager.LoadScene(name);
                });
            }

            //Warning message
            layout.CreateMargin();
            layout.CreateToggleButton("Clear Services", _clearServices, value => {
                _clearServices = value;
            });
            var infoLabel = layout.CreateLabel("Warning: Scene changes are not always clean. Remaining data from previous scenes can persist and create errors");
            infoLabel.fontSize /= 2;//Set info text as half the size of default label text
            infoLabel.fontStyle = FontStyle.Italic;
        }
    }
}

#endif
