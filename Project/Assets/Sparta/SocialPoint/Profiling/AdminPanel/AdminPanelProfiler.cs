#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Profiling
{
    public sealed class FrameInfoGUI : IFloatingPanelGUI, IUpdateable
    {
        PerfInfo _info;
        Text _text;

        public FrameInfoGUI(PerfInfo info)
        {
            _info = info;
        }

        public void OnCreateFloatingPanel(FloatingPanelController panel)
        {
            panel.Title = "FrameInfo";
            panel.Size = new Vector2(200, 120);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _text = layout.CreateTextArea(_info.Frame.ToString());
            layout.RegisterUpdateable(this);
        }

        public void Update()
        {
            if(_text != null)
            {
                _text.text = _info.Frame.ToString();
            }
        }
    }

    public sealed class AdminPanelProfiler : IAdminPanelGUI, IAdminPanelConfigurer
    {
        PerfInfo _info;
        PerfInfoGUI _gui;
        Text _frameText;
        Text _garbageText;
        Text _deviceInfoText;

        public AdminPanelProfiler()
        {
        }

        public AdminPanelProfiler(PerfInfo info)
        {
            _info = info;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _gui = GameObject.FindObjectOfType<PerfInfoGUI>();
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Profiler", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            if(_info == null && _gui != null)
            {
                _info = _gui.Info;
            }
            if(_gui != null)
            {
                layout.CreateToggleButton("Show performance info", _gui.enabled, (value) => {
                    _gui.enabled = value;
                });
            }
            if(_info != null)
            {
                layout.CreateButton("Show frame info", () => {
                    FloatingPanelController.Create(new FrameInfoGUI(_info)).Show();
                });

                layout.CreateLabel("Frame Info");
                _frameText = layout.CreateVerticalScrollLayout().CreateTextArea(_info.Frame.ToString());

                layout.CreateLabel("Garbage Info");
                _garbageText = layout.CreateVerticalScrollLayout().CreateTextArea(_info.Garbage.ToString());

                layout.CreateLabel("Device Info");
                _deviceInfoText = layout.CreateVerticalScrollLayout().CreateTextArea(_info.Device.ToString());

                layout.CreateMargin();
                layout.CreateButton("Refresh", UpdateContent);
            }
        }

        void UpdateContent()
        {
            if(_info != null)
            {
                _frameText.text = _info.Frame.ToString();
                _garbageText.text = _info.Garbage.ToString();
                _deviceInfoText.text = _info.Device.ToString();
            }
        }
    }
}

#endif
