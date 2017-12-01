#if ADMIN_PANEL 

using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using UnityEngine.UI;
using UnityEngine;
using SocialPoint.Attributes;
using SocialPoint.Dependency;
using SocialPoint.Hardware;

namespace SocialPoint.GUIControl
{
    public sealed class AdminPanelUI : IAdminPanelGUI, IAdminPanelConfigurer
    {
        const string kShowSafeArea = "ShowSafeArea";
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        string _safeAreaX;
        string _safeAreaY;
        string _safeAreaWidth;
        string _safeAreaHeight;
        bool _showSafeArea;

        IAttrStorage _storage;
        UIStackController _stackController;
        UISafeAreaController _safeAreaController;
        IDeviceInfo _deviceInfo;

        public AdminPanelUI(IDeviceInfo deviceInfo, UISafeAreaController safeAreaController, IAttrStorage persistentStorage, UIStackController stackController)
        {
            _deviceInfo = deviceInfo;
            _safeAreaController = safeAreaController;
            _storage = persistentStorage;
            _stackController = stackController;
        }
            
        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("UI", this));
        }

        #endregion

        #region IAdminPanelGUI implementation

        AdminPanelLayout _layout;

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;
            _layout.CreateLabel("Safe Area");

            if(_showSafeArea)
            {
                _layout.CreateButton("Hide safe area", HideSafeArea);

                _layout.CreateTextArea("Current safe area: " + _safeAreaController.GetSafeAreaRect());

//                var lhlayout = _layout.CreateHorizontalLayout();
//                lhlayout.CreateLabel("Left");
//                lhlayout.CreateLabel("Top");
//                lhlayout.CreateLabel("Width");
//                lhlayout.CreateLabel("Height");

//                var hlayout = _layout.CreateHorizontalLayout();
//                hlayout.CreateTextInput(_safeAreaX, OnKeyXSubmitted);
//                hlayout.CreateTextInput(_safeAreaY, OnKeyYSubmitted);
//                hlayout.CreateTextInput(_safeAreaWidth, OnKeyWidthSubmitted);
//                hlayout.CreateTextInput(_safeAreaHeight, OnKeyHeightSubmitted);
//
//                _layout.CreateButton("Set custom safe area", SetCustomSafeArea);

                _layout.CreateMargin();

                _layout.CreateButton("Set whole screen as safe area", SetWholeScreenSafeArea);

#if UNITY_IOS
                _layout.CreateButton("Set Default Safe area", SetDefaultSafeArea);    
#endif
            }
            else
            {
                _layout.CreateButton("Show safe area", ShowSafeArea);
            }
        }

        void SaveCustomSafeArea()
        {
            float valueX;
            float.TryParse(_safeAreaX, out valueX);

            float valueY;
            float.TryParse(_safeAreaY, out valueY);

            float valueWidth;
            float.TryParse(_safeAreaWidth, out valueWidth);

            float valueHeight;
            float.TryParse(_safeAreaHeight, out valueHeight);

            _storage.Save(kCustomX, new AttrString(_safeAreaX));
            _storage.Save(kCustomY, new AttrString(_safeAreaY));
            _storage.Save(kCustomWidth, new AttrString(_safeAreaWidth));
            _storage.Save(kCustomHeight, new AttrString(_safeAreaHeight));
        }

        void OnKeyXSubmitted(string value)
        {
            _safeAreaX = value;
            _storage.Save(kCustomX, new AttrString(_safeAreaX));

            _layout.Refresh();
        }

        void OnKeyYSubmitted(string value)
        {
            _safeAreaY = value;
            _storage.Save(kCustomY, new AttrString(_safeAreaY));

            _layout.Refresh();
        }

        void OnKeyWidthSubmitted(string value)
        {
            _safeAreaWidth = value;
            _storage.Save(kCustomWidth, new AttrString(_safeAreaWidth));

            _layout.Refresh();
        }

        void OnKeyHeightSubmitted(string value)
        {
            _safeAreaHeight = value;
            _storage.Save(kCustomHeight, new AttrString(_safeAreaHeight));

            _layout.Refresh();
        }

        #endregion

        void ShowSafeArea()
        {
            _showSafeArea = true;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            RefreshUIViewControllersSafeArea();
        }

        void HideSafeArea()
        {
            _showSafeArea = false;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            SetWholeScreenSafeArea();
        }
            
        void SetWholeScreenSafeArea()
        {
            var rect = new Rect(0f, 0f, _deviceInfo.ScreenSize.x, _deviceInfo.ScreenSize.y);

            _safeAreaX = rect.x.ToString();
            _safeAreaY = rect.y.ToString();
            _safeAreaWidth = rect.width.ToString();
            _safeAreaHeight = rect.height.ToString();

            SaveCustomSafeArea();
            RefreshUIViewControllersSafeArea();
        }

        void SetDefaultSafeArea()
        {
            var rect = _deviceInfo.SafeAreaRectSize;

            _safeAreaX = rect.x.ToString();
            _safeAreaY = rect.y.ToString();
            _safeAreaWidth = rect.width.ToString();
            _safeAreaHeight = rect.height.ToString();

            SaveCustomSafeArea();
            RefreshUIViewControllersSafeArea();
        }

        void RefreshUIViewControllersSafeArea()
        {
            _layout.Refresh();

            if(_stackController != null)
            {
                _stackController.RefreshSafeArea(_safeAreaController.GetSafeAreaRect());
            }
        }
    }
}

#endif
