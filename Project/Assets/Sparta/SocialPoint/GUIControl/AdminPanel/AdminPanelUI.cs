#if ADMIN_PANEL 

using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using UnityEngine.UI;
using UnityEngine;
using SocialPoint.Attributes;

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
        Rect _customArea;
        Rect _screenArea;

        string _customX;
        string _customY;
        string _customWidth;
        string _customHeight;
        bool _showSafeArea;

        IAttrStorage _storage;

        public AdminPanelUI(Rect safeArea, IAttrStorage persistentStorage)
        {
            _safeArea = safeArea;
            _storage = persistentStorage;

            if(_storage.Has(kShowSafeArea))
            {
                _showSafeArea = _storage.Load(kShowSafeArea).AsValue.ToBool();   
            }
            else
            {
                _showSafeArea = false;
            }

            if(_storage.Has(kCustomX))
            {
                _customX = _storage.Load(kCustomX).AsValue.ToString();   
            }
            else
            {
                _customX = safeArea.x.ToString();
            }

            if(_storage.Has(kCustomY))
            {
                _customY = _storage.Load(kCustomY).AsValue.ToString();   
            }
            else
            {
                _customY = safeArea.y.ToString();
            }

            if(_storage.Has(kCustomWidth))
            {
                _customWidth = _storage.Load(kCustomWidth).AsValue.ToString();   
            }
            else
            {
                _customWidth = safeArea.width.ToString();    
            }

            if(_storage.Has(kCustomHeight))
            {
                _customHeight = _storage.Load(kCustomHeight).AsValue.ToString();   
            }
            else
            {
                _customHeight = safeArea.height.ToString();
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

                var content = "Current safe area: " + _customArea;
                _layout.CreateTextArea(content.ToString());

                var lhlayout = _layout.CreateHorizontalLayout();
                lhlayout.CreateLabel("Left");
                lhlayout.CreateLabel("Top");
                lhlayout.CreateLabel("Width");
                lhlayout.CreateLabel("Height");

                var hlayout = _layout.CreateHorizontalLayout();
                hlayout.CreateTextInput(_customX, OnKeyXSubmitted);
                hlayout.CreateTextInput(_customY, OnKeyYSubmitted);
                hlayout.CreateTextInput(_customWidth, OnKeyWidthSubmitted);
                hlayout.CreateTextInput(_customHeight, OnKeyHeightSubmitted);

                _layout.CreateButton("Set custom safe area", SetCustomSafeArea);

                _layout.CreateMargin();

                _layout.CreateButton("Reset to default: " + _safeArea, SetDefaultSafeArea);
                _layout.CreateButton("Use the whole screen", SetWholeScreenSafeArea);
            }
            else
            {
                _layout.CreateButton("Show safe area", ShowSafeArea);
            }
        }

        void CreateCustomSafeArea()
        {
            float valueX;
            float.TryParse(_customX, out valueX);

            float valueY;
            float.TryParse(_customY, out valueY);

            float valueWidth;
            float.TryParse(_customWidth, out valueWidth);

            float valueHeight;
            float.TryParse(_customHeight, out valueHeight);

            _customArea = new Rect(valueX, valueY, valueWidth, valueHeight);

            _storage.Save(kCustomX, new AttrString(_customX));
            _storage.Save(kCustomY, new AttrString(_customY));
            _storage.Save(kCustomWidth, new AttrString(_customWidth));
            _storage.Save(kCustomHeight, new AttrString(_customHeight));
        }

        void OnKeyXSubmitted(string value)
        {
            _customX = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomX, new AttrString(_customX));

            _layout.Refresh();
        }

        void OnKeyYSubmitted(string value)
        {
            _customY = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomY, new AttrString(_customY));

            _layout.Refresh();
        }

        void OnKeyWidthSubmitted(string value)
        {
            _customWidth = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomWidth, new AttrString(_customWidth));

            _layout.Refresh();
        }

        void OnKeyHeightSubmitted(string value)
        {
            _customHeight = value;
            CreateCustomSafeArea();
            _storage.Save(kCustomHeight, new AttrString(_customHeight));

            _layout.Refresh();
        }

        #endregion

        void ShowSafeArea()
        {
            _showSafeArea = true;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            _layout.Refresh();
        }

        void HideSafeArea()
        {
            _showSafeArea = false;
            _storage.Save(kShowSafeArea, new AttrString(_showSafeArea.ToString()));

            _layout.Refresh();
        }

        void SetCustomSafeArea()
        {
            ApplySafeArea();
        }

        void SetDefaultSafeArea()
        {
            _customX = _safeArea.x.ToString();
            _customY = _safeArea.y.ToString();
            _customWidth = _safeArea.width.ToString();
            _customHeight = _safeArea.height.ToString();

            CreateCustomSafeArea();

            ApplySafeArea();
        }

        void SetWholeScreenSafeArea()
        {
            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            _layout.Refresh();

            if(_showSafeArea)
            {
                
            }
        }
    }
}

#endif
