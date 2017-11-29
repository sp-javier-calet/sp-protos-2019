#if ADMIN_PANEL 

using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using UnityEngine.UI;
using UnityEngine;
using SocialPoint.Attributes;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public sealed class AdminPanelUI : IAdminPanelGUI, IAdminPanelConfigurer
    {
        const string kShowSafeArea = "ShowSafeArea";
        const string kCustomX = "CustomSafeAreaX";
        const string kCustomY = "CustomSafeAreaY";
        const string kCustomWidth = "CustomSafeAreaWidth";
        const string kCustomHeight = "CustomSafeAreaHeight";

        Rect _safeArea;
        Rect _screenArea;
        Rect _iPhoneXSafeArea;

        string _safeAreaX;
        string _safeAreaY;
        string _safeAreaWidth;
        string _safeAreaHeight;
        bool _showSafeArea;

        IAttrStorage _storage;
        UIStackController _stackController;

        public AdminPanelUI(Rect screenArea, Rect iPhoneXSafeArea, Rect safeArea, IAttrStorage persistentStorage, UIStackController stackController)
        {
            _screenArea = screenArea;
            _iPhoneXSafeArea = iPhoneXSafeArea;
            _safeArea = safeArea;
            _storage = persistentStorage;
            _stackController = stackController;

            if(_storage.Has(kShowSafeArea))
            {
                _showSafeArea = _storage.Load(kShowSafeArea).AsValue.ToBool();   
            }
            else
            {
                _showSafeArea = false;
            }

            var xAttr = _storage.Load(kCustomX);
            if(xAttr != null)
            {
                _safeAreaX = xAttr.AsValue.ToString();  
            }
            else
            {
                _safeAreaX = safeArea.x.ToString();
            }

            var yAttr = _storage.Load(kCustomY);
            if(yAttr != null)
            {
                _safeAreaY = yAttr.AsValue.ToString(); 
            }
            else
            {
                _safeAreaY = safeArea.y.ToString();
            }

            var widthAttr = _storage.Load(kCustomWidth);
            if(widthAttr != null)
            {
                _safeAreaWidth = widthAttr.AsValue.ToString(); 
            }
            else
            {
                _safeAreaWidth = safeArea.width.ToString();
            }

            var heightAttr = _storage.Load(kCustomHeight);
            if(heightAttr != null)
            {
                _safeAreaHeight = heightAttr.AsValue.ToString(); 
            }
            else
            {
                _safeAreaHeight = safeArea.height.ToString();
            }

            CreateCustomSafeArea();
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

                var content = "Current safe area: " + _safeArea;
                _layout.CreateTextArea(content.ToString());

                var lhlayout = _layout.CreateHorizontalLayout();
                lhlayout.CreateLabel("Left");
                lhlayout.CreateLabel("Top");
                lhlayout.CreateLabel("Width");
                lhlayout.CreateLabel("Height");

                var hlayout = _layout.CreateHorizontalLayout();
                hlayout.CreateTextInput(_safeAreaX, OnKeyXSubmitted);
                hlayout.CreateTextInput(_safeAreaY, OnKeyYSubmitted);
                hlayout.CreateTextInput(_safeAreaWidth, OnKeyWidthSubmitted);
                hlayout.CreateTextInput(_safeAreaHeight, OnKeyHeightSubmitted);

                _layout.CreateButton("Set custom safe area", SetCustomSafeArea);

                _layout.CreateMargin();

                _layout.CreateButton("Set whole screen as safe area", SetWholeScreenSafeArea);

#if UNITY_IOS
                _layout.CreateButton("Set IphoneX safe area", SetIphoneXSafeArea);    
#endif
            }
            else
            {
                _layout.CreateButton("Show safe area", ShowSafeArea);
            }
        }

        void CreateCustomSafeArea()
        {
            float valueX;
            float.TryParse(_safeAreaX, out valueX);

            float valueY;
            float.TryParse(_safeAreaY, out valueY);

            float valueWidth;
            float.TryParse(_safeAreaWidth, out valueWidth);

            float valueHeight;
            float.TryParse(_safeAreaHeight, out valueHeight);

            _safeArea = new Rect(valueX, valueY, valueWidth, valueHeight);

            _storage.Save(kCustomX, new AttrString(_safeAreaX));
            _storage.Save(kCustomY, new AttrString(_safeAreaY));
            _storage.Save(kCustomWidth, new AttrString(_safeAreaWidth));
            _storage.Save(kCustomHeight, new AttrString(_safeAreaHeight));
        }

        void OnKeyXSubmitted(string value)
        {
            _safeAreaX = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomX, new AttrString(_safeAreaX));

            _layout.Refresh();
        }

        void OnKeyYSubmitted(string value)
        {
            _safeAreaY = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomY, new AttrString(_safeAreaY));

            _layout.Refresh();
        }

        void OnKeyWidthSubmitted(string value)
        {
            _safeAreaWidth = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomWidth, new AttrString(_safeAreaWidth));

            _layout.Refresh();
        }

        void OnKeyHeightSubmitted(string value)
        {
            _safeAreaHeight = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomHeight, new AttrString(_safeAreaHeight));

            _layout.Refresh();
        }

        #endregion

        void ShowSafeArea()
        {
            _showSafeArea = true;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            SetCustomSafeArea();
        }

        void HideSafeArea()
        {
            _showSafeArea = false;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            SetWholeScreenSafeArea();
        }

        void SetCustomSafeArea()
        {
            ApplySafeArea();
        }

        void SetWholeScreenSafeArea()
        {
            _safeAreaX = _screenArea.x.ToString();
            _safeAreaY = _screenArea.y.ToString();
            _safeAreaWidth = _screenArea.width.ToString();
            _safeAreaHeight = _screenArea.height.ToString();

            CreateCustomSafeArea();

            ApplySafeArea();
        }

        void SetIphoneXSafeArea()
        {
            _safeAreaX = _iPhoneXSafeArea.x.ToString();
            _safeAreaY = _iPhoneXSafeArea.y.ToString();
            _safeAreaWidth = _iPhoneXSafeArea.width.ToString();
            _safeAreaHeight = _iPhoneXSafeArea.height.ToString();

            CreateCustomSafeArea();

            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            _layout.Refresh();

            if(_stackController != null)
            {
                _stackController.RefreshSafeArea();
            }
        }
    }
}

#endif
