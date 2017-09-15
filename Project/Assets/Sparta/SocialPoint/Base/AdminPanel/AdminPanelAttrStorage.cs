#if ADMIN_PANEL 

using SocialPoint.AdminPanel;
using SocialPoint.Attributes;

namespace SocialPoint.Base
{
    public class AdminPanelAttrStorage : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly string _name;
        readonly IAttrStorage _storage;
        readonly IAttrSerializer _serializer;
        AdminPanelConsole _console;
        string _key = "unexisting_key";
        string _value;

        public AdminPanelAttrStorage(string name, IAttrStorage storage)
        {
            _name = name;
            _storage = storage;
            _serializer = new JsonAttrSerializer();
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI(string.Format("AttrStorage '{0}'", _name), this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel(string.Format("IAttrStorage ({0})", _storage.GetType().Name));
            var hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Key");
            hlayout.CreateTextInput(_key, value => 
                {
                    _key = value;
                });

            hlayout = layout.CreateHorizontalLayout();
            hlayout.CreateFormLabel("Value");
            hlayout.CreateTextInput(_value, value => 
                {
                    _value = value;
                });

            layout.CreateMargin();
            
            layout.CreateButton("Has Value", () => {
                var has = _storage.Has(_key);
                _console.Print(string.Format("'{0}' has key '{1}': {2}", _name, _key, has));
            });

            layout.CreateButton("Get Value", () => {
                var attr = _storage.Load(_key);
                if(attr != null)
                {
                    var serialized = _serializer.SerializeString(attr);
                    _console.Print(string.Format("Loaded '{0}' key '{1}' with value: {2}", _name, _key, serialized));
                }
                else
                {
                    _console.Print(string.Format("Loaded '{0}' key '{1}' with no value", _name, _key)); 
                }
            });

            layout.CreateButton("Set Value", () => {
                _storage.Save(_key, new AttrString(_value));
                _console.Print(string.Format("Saved '{0}' key '{1}' with value: {2}", _name, _key, _value));
            });
        }
    }
}

#endif
