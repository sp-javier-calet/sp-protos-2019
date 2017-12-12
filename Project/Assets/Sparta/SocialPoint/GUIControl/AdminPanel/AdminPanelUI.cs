#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using UnityEngine;
using SocialPoint.Attributes;
using SocialPoint.Hardware;
using UnityEngine.UI;

namespace SocialPoint.GUIControl
{
    public sealed class AdminPanelUI : IAdminPanelGUI, IAdminPanelConfigurer
    {
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        float _safeAreaX;
        float _safeAreaY;
        float _safeAreaWidth;
        float _safeAreaHeight;

        InputField _safeAreaXInput;
        InputField _safeAreaYInput;
        InputField _safeAreaWidthInput;
        InputField _safeAreaHeightInput;

        readonly IAttrStorage _storage;
        readonly IDeviceInfo _deviceInfo;

        public AdminPanelUI(IDeviceInfo deviceInfo, IAttrStorage persistentStorage)
        {
            _deviceInfo = deviceInfo;
            _storage = persistentStorage;

            LoadCustomSafeArea();
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

            var lhlayout = _layout.CreateHorizontalLayout();
            lhlayout.CreateLabel("Left");
            lhlayout.CreateLabel("Top");
            lhlayout.CreateLabel("Width");
            lhlayout.CreateLabel("Height");

            var hlayout = _layout.CreateHorizontalLayout();
            _safeAreaXInput = hlayout.CreateTextInput(_safeAreaX.ToString(), OnKeyXSubmitted);
            _safeAreaYInput = hlayout.CreateTextInput(_safeAreaY.ToString(), OnKeyYSubmitted);
            _safeAreaWidthInput = hlayout.CreateTextInput(_safeAreaWidth.ToString(), OnKeyWidthSubmitted);
            _safeAreaHeightInput = hlayout.CreateTextInput(_safeAreaHeight.ToString(), OnKeyHeightSubmitted);

            _layout.CreateButton("Use custom Safe Area", SetCustomSafeArea);
            _layout.CreateMargin();

            _layout.CreateButton("Use device Safe Area", SetDefaultSafeArea);
            _layout.CreateButton("Disable Safe Area", SetWholeScreenSafeArea);
        }

        void OnKeyXSubmitted(string value)
        {
            float.TryParse(_safeAreaXInput.text, out _safeAreaX);
            _storage.Save(kCustomX, new AttrFloat(_safeAreaX));
        }

        void OnKeyYSubmitted(string value)
        {
            float.TryParse(_safeAreaYInput.text, out _safeAreaY);
            _storage.Save(kCustomY, new AttrFloat(_safeAreaY));
        }

        void OnKeyWidthSubmitted(string value)
        {
            float.TryParse(_safeAreaWidthInput.text, out _safeAreaWidth);
            _storage.Save(kCustomWidth, new AttrFloat(_safeAreaWidth));
        }

        void OnKeyHeightSubmitted(string value)
        {
            float.TryParse(_safeAreaHeightInput.text, out _safeAreaHeight);
            _storage.Save(kCustomHeight, new AttrFloat(_safeAreaHeight));
        }

        void LoadCustomSafeArea()
        {
            var xAttr = _storage.Load(kCustomX);
            _safeAreaX = xAttr != null ? xAttr.AsValue.ToFloat() : 0f;  

            var yAttr = _storage.Load(kCustomY);
            _safeAreaY = yAttr != null ? yAttr.AsValue.ToFloat() : 0f; 

            var widthAttr = _storage.Load(kCustomWidth);
            _safeAreaWidth = widthAttr != null ? widthAttr.AsValue.ToFloat() : _deviceInfo.ScreenSize.x; 

            var heightAttr = _storage.Load(kCustomHeight);
            _safeAreaHeight = heightAttr != null ? heightAttr.AsValue.ToFloat() : _deviceInfo.ScreenSize.y; 
        }

        void SaveCustomSafeArea()
        {
            _layout.Refresh();

            _storage.Save(kCustomX, new AttrFloat(_safeAreaX));
            _storage.Save(kCustomY, new AttrFloat(_safeAreaY));
            _storage.Save(kCustomWidth, new AttrFloat(_safeAreaWidth));
            _storage.Save(kCustomHeight, new AttrFloat(_safeAreaHeight));
        }
            
        #endregion

        void SetWholeScreenSafeArea()
        {
            _safeAreaX = 0f;
            _safeAreaY = 0f;
            _safeAreaWidth = _deviceInfo.ScreenSize.x;
            _safeAreaHeight = _deviceInfo.ScreenSize.y;

            SaveCustomSafeArea();
            ApplySafeArea();
        }

        void SetCustomSafeArea()
        {
            SaveCustomSafeArea();
            ApplySafeArea();
        }

        void SetDefaultSafeArea()
        {
            _safeAreaX = _deviceInfo.SafeAreaRectSize.x;
            _safeAreaY = _deviceInfo.SafeAreaRectSize.y;
            _safeAreaWidth = _deviceInfo.SafeAreaRectSize.width;
            _safeAreaHeight = _deviceInfo.SafeAreaRectSize.height;

            SaveCustomSafeArea();
            ApplySafeArea();
        }
            
        void ApplySafeArea()
        {
            var rect = new Rect(_safeAreaX, _safeAreaY, _safeAreaWidth, _safeAreaHeight);
            var ratioX = Screen.width / _deviceInfo.ScreenSize.x;
            var ratioY = Screen.height / _deviceInfo.ScreenSize.y;

            var finalRect = new Rect(rect.x * ratioX, rect.y * ratioY, rect.width * ratioX, rect.height * ratioY);

            var views = Object.FindObjectsOfType<UISafeAreaViewController>();
            for(int i = 0; i < views.Length; ++i)
            {
                var view = views[i];
                if(view != null)
                {
                    view.ApplySafeArea(finalRect);
                }
            }
        }
    }
}

#endif
