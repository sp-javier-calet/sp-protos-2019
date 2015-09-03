using UnityEngine;
using UnityEngine.UI;
using SocialPoint.AdminPanel;

namespace SocialPoint.Profiler
{
    public class AdminPanelProfilerGUI : AdminPanelGUI, AdminPanelConfigurer
    {
        private PerfInfoGUI _perfInfoGUI;
        private Text _frameText;
        private Text _garbageText;
        
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
                    _perfInfoGUI.enabled = value; });

                layout.CreateButton("Refresh", () => { UpdateContent(); });
                layout.CreateMargin();

                layout.CreateLabel("Frame Info");
                layout.CreateVerticalScrollLayout().CreateTextArea(_perfInfoGUI.Info.Frame.ToString(), out _frameText);

                layout.CreateLabel("Garbage Info");
                layout.CreateVerticalScrollLayout().CreateTextArea(_perfInfoGUI.Info.Garbage.ToString(), out _garbageText);
            }
        }

        private void UpdateContent()
        {
            if(_perfInfoGUI != null)
            {
                _frameText.text = _perfInfoGUI.Info.Frame.ToString();
                _garbageText.text = _perfInfoGUI.Info.Garbage.ToString();
            }
        }
    }
}