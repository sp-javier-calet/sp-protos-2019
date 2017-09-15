#if ADMIN_PANEL 

using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Console;
using UnityEngine.UI;

namespace SocialPoint.Locale
{
    public sealed class AdminPanelLocale : IAdminPanelGUI, IAdminPanelConfigurer
    {
        readonly ILocalizationManager _manager;
        AdminPanelConsole _console;
        Dictionary<string, Localization> _locales;

        public AdminPanelLocale(ILocalizationManager manager)
        {
            _manager = manager;
            _manager.Loaded += OnLanguagesLoaded;
        }

        #region IAdminPanelConfigurer implementation

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Locale", this));
            var cmd = new ConsoleCommand()
                .WithDescription("get a localized string")
                .WithDelegate(OnTranslateCommand);
            adminPanel.RegisterCommand("translate", cmd);
        }

        #endregion

        #region IAdminPanelGUI implementation

        Dictionary<string,Toggle> _langButtons;
        AdminPanelLayout _layout;
        LocalizationManager _lm;
        string _translatedKey;
        string _translateInputSavedText = "HUD_ALLIANCE_POPUP_CREATE_NOT_COINS_EMPTY_KEY";

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            _layout = layout;

            _lm = _manager as LocalizationManager;
            if(_lm == null)
            {
                _layout.CreateLabel("Unknown Localization Manager Implementation");
                return;
            }

            _layout.CreateLabel("Locale Info");
            var content = new StringBuilder();
            content.AppendLine(GetLocalizationPaths());
            content.AppendLine("LANGUAGES:\n--------------------------------------------------------------");
            content.AppendLine(GetLocalizationLocales());
            content.AppendLine(GetLocalizationLocales(true));
            content.AppendFormat("Selected Language: {0}\n", _lm.SelectedLanguage);
            content.AppendFormat("Selected CultureInfo: {0}\n", _lm.SelectedCultureInfo);
            content.AppendFormat("Current Language: {0}\n", _lm.CurrentLanguage);
            content.AppendFormat("Current CultureInfo: {0}\n", _lm.CurrentCultureInfo);

            _layout.CreateVerticalLayout().CreateTextArea(content.ToString());

            _layout.CreateToggleButton("Show Keys On DevMode", _lm.Localization.ShowKeysOnDevMode, value => {
                _lm.Localization.ShowKeysOnDevMode = value;
                _layout.Refresh();
            });
            _layout.CreateButton("DownloadSupportedLanguages", DownloadSupportedLanguages);
            _layout.CreateMargin();

            var flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Translate Key:");
            flayout.CreateTextInput(_translateInputSavedText, OnKeySubmitted);
            _translatedKey = _manager.Localization.Get(_translateInputSavedText, "<key not found>");
            flayout = layout.CreateHorizontalLayout();
            flayout.CreateFormLabel("Translation:");
            flayout.CreateLabel(_translatedKey ?? string.Empty);

            layout.CreateLabel("Change Language (input language > loads file)");
            _langButtons = new Dictionary<string,Toggle>();
            for(int i = 0, _managerSupportedLanguagesLength = _manager.SupportedLanguages.Length; i < _managerSupportedLanguagesLength; i++)
            {
                var lang = _manager.SupportedLanguages[i];
                var fixedLang = Reflection.CallPrivateMethod<LocalizationManager, string>(_lm, "FixLanguage", lang);
                var downloadedLang = _locales != null && _locales.ContainsKey(fixedLang) && _locales[fixedLang].Strings.Count > 0;
                var buttonLabel = string.Format("{0} > {1}", lang, fixedLang);
                _langButtons[lang] = layout.CreateToggleButton(buttonLabel, true, onToggle => {
                    _manager.CurrentLanguage = lang;
                    _layout.Refresh();
                }
                    , downloadedLang
                );
            }

            var itr = _langButtons.GetEnumerator();
            while(itr.MoveNext())
            {
                var lang = itr.Current.Key;
                var fixedLang = Reflection.CallPrivateMethod<LocalizationManager, string>(_lm, "FixLanguage", lang);
                var toggle = itr.Current.Value;

                var action = toggle.onValueChanged;
                toggle.onValueChanged = new Toggle.ToggleEvent();
                toggle.isOn = fixedLang == _manager.CurrentLanguage;
                toggle.onValueChanged = action;
            }
            itr.Dispose();
        }

        void OnKeySubmitted(string value)
        {
            _translatedKey = _manager.Localization.Get(value, "<key not found>");
            _translateInputSavedText = value;
            _layout.Refresh();
        }

        string GetLocalizationPaths()
        {
            var cachePath = Reflection.GetPrivateField<LocalizationManager, string>(_manager, "_cachePath");
            var bundlePath = Reflection.GetPrivateField<LocalizationManager, string>(_manager, "_bundlePath");

            var content = new StringBuilder();

            content.AppendLine("PATHS:\n-------------------------------------------------------------------");
            content.AppendLine("Cache Path\n" + cachePath);
            content.AppendLine("Bundle Path\n" + bundlePath);

            return content.ToString();
        }

        string GetLocalizationLocales(bool downloadedLocalesOnly = false)
        {
            if(_locales == null)
            {
                return string.Empty;
            }

            var content = new StringBuilder();
            content.Append(downloadedLocalesOnly ? "Downloaded: " : "Tried to download: ");

            var itr = _locales.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                if(!downloadedLocalesOnly)
                {
                    content.Append(item.Key);
                }
                else if(item.Value.Strings.Count > 0) // only downloaded locales
                {
                    content.Append(item.Key);
                }
                content.Append(" ");
            }
            itr.Dispose();

            return content.ToString();
        }

        #endregion

        void DownloadSupportedLanguages()
        {
            Reflection.CallPrivateVoidMethod<LocalizationManager>(_lm, "DownloadSupportedLanguages", null, null);
        }

        void OnLanguagesLoaded(Dictionary<string, Localization> locales)
        {
            _locales = locales;
            if(_layout != null)
            {
                _layout.Refresh();
            }
        }

        void OnTranslateCommand(ConsoleCommand cmd)
        {
            var list = new List<string>(cmd.Arguments);
            if(list.Count == 0)
            {
                throw new ConsoleException("Need at least a key argument");
            }
            var trans = _manager.Localization.Get(list[0]);
            list.RemoveAt(0);
            if(list.Count > 0)
            {
                trans = string.Format(trans, list.ToArray());
            }
            if(_console != null)
            {
                _console.Print(trans);
            }
        }
    }

}

#endif
