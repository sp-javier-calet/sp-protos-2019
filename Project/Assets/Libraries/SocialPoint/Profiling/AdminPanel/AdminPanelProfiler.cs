using SocialPoint.AdminPanel;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Profiling
{
    public class AdminPanelProfiler : IAdminPanelGUI, IAdminPanelConfigurer
    {
        PerfInfoGUI _perfInfoGUI;
        Text _frameText;
        Text _garbageText;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _perfInfoGUI = GameObject.FindObjectOfType<PerfInfoGUI>();

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Profiler", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_perfInfoGUI != null)
            {
                layout.CreateToggleButton("Show performance info", _perfInfoGUI.enabled, (value) => {
                    _perfInfoGUI.enabled = value;
                });

                layout.CreateButton("Refresh", UpdateContent);
                layout.CreateMargin();

                layout.CreateLabel("Frame Info");
                _frameText = layout.CreateVerticalScrollLayout().CreateTextArea(_perfInfoGUI.Info.Frame.ToString());

                layout.CreateLabel("Garbage Info");
                _garbageText = layout.CreateVerticalScrollLayout().CreateTextArea(_perfInfoGUI.Info.Garbage.ToString());
            }
            else
            {
                layout.CreateLabel("Performance info unavailable. There is no PerfInfoGUI object in the current scene");
            }
        }

        void UpdateContent()
        {
            if(_perfInfoGUI != null)
            {
                _frameText.text = _perfInfoGUI.Info.Frame.ToString();
                _garbageText.text = _perfInfoGUI.Info.Garbage.ToString();
            }
        }
    }
}
