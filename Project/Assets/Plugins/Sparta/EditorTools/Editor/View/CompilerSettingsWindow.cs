using UnityEngine;
using UnityEditor;
using System.IO;
using SpartaTools.Editor.Build;

namespace SpartaTools.Editor.View
{
    public class CompilerSettingsWindow : ISubWindow
    {
        const string WarningAsErrorsEnabledKey = "SpartaEditorWarningAsErrorsEnabled";
        const string EnabledWarningAsErrorsKey = "SpartaEditorEnabledWarningAsErrors";
        const string DisabledWarningAsErrorsKey = "SpartaEditorDisabledWarningAsErrors";
        const string GlobalDefinedSymbolsKey = "SpartaEditorGlobalDefinedSymbolsErrors";


        class CompilerData
        {
            public bool EnabledWarningAsErrors;
            public string EnabledWarnings;
            public string DisabledWarnings;
            public string DefinedSymbols;
        }

        CompilerData CurrentSettings;

        CompilerData Load()
        {
            var data = new CompilerData();
            data.EnabledWarningAsErrors = EditorPrefs.GetBool(WarningAsErrorsEnabledKey, false);
            data.EnabledWarnings = EditorPrefs.GetString(EnabledWarningAsErrorsKey, string.Empty);
            data.DisabledWarnings = EditorPrefs.GetString(DisabledWarningAsErrorsKey, string.Empty);
            data.DefinedSymbols = EditorPrefs.GetString(GlobalDefinedSymbolsKey, string.Empty);

            return data;
        }

        void Save(CompilerData data)
        {
            EditorPrefs.SetBool(WarningAsErrorsEnabledKey, data.EnabledWarningAsErrors);
            EditorPrefs.SetString(EnabledWarningAsErrorsKey, data.EnabledWarnings);
            EditorPrefs.SetString(DisabledWarningAsErrorsKey, data.DisabledWarnings);
            EditorPrefs.SetString(GlobalDefinedSymbolsKey, data.DefinedSymbols);
        }

        void SetCompilerSettings(CompilerData data)
        {
            var settings = new CompilerSettingsFile();

            settings.WarningAsErrors.Enable = data.EnabledWarningAsErrors;

            var separators = new char[] { ' ', ';', ',' };

            var codes = data.EnabledWarnings.Split(separators);
            for(var i = 0; i < codes.Length; ++i)
            {
                int value = 0;
                if(int.TryParse(codes[i], out value))
                {
                    settings.WarningAsErrors.EnableWarning(value);
                }
            }

            codes = data.DisabledWarnings.Split(separators);
            for(var i = 0; i < codes.Length; ++i)
            {
                int value = 0;
                if(int.TryParse(codes[i], out value))
                {
                    settings.WarningAsErrors.DisableWarning(value);
                }
            }

            var symbols = data.DefinedSymbols.Split(separators);
            for(var i = 0; i < symbols.Length; ++i)
            {
                var symbol = symbols[i];
                if(!string.IsNullOrEmpty(symbol))
                {
                    settings.DefinedSymbols.Add(symbol);
                }
            }

            settings.Save();
        }

        #region Draw GUI

        public void OnGUI()
        {
            if(CurrentSettings == null)
            {
                CurrentSettings = Load();
            }

            EditorGUILayout.LabelField("Mono Compiler Settings", EditorStyles.boldLabel);
            var globalWarningAsError = EditorGUILayout.Toggle("Warning as Errors", CurrentSettings.EnabledWarningAsErrors);
            var enabled = EditorGUILayout.TextField("Enabled Warnings", CurrentSettings.EnabledWarnings);
            var disabled = EditorGUILayout.TextField("Disabled Warnings", CurrentSettings.DisabledWarnings);
            var symbols = EditorGUILayout.TextField("Defined symbols", CurrentSettings.DefinedSymbols);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Set Default", EditorStyles.miniButton))
            {
                globalWarningAsError = true;
                enabled = string.Empty;
                disabled = "618,169,649";
                symbols = string.Empty;
            }
            GUILayout.EndHorizontal();

            var statusChanged = CurrentSettings.EnabledWarningAsErrors != globalWarningAsError;

            if(statusChanged ||
               CurrentSettings.EnabledWarnings != enabled ||
               CurrentSettings.DisabledWarnings != disabled ||
               CurrentSettings.DefinedSymbols != symbols)
            {
                CurrentSettings.EnabledWarningAsErrors = globalWarningAsError;
                CurrentSettings.EnabledWarnings = enabled;
                CurrentSettings.DisabledWarnings = disabled;
                CurrentSettings.DefinedSymbols = symbols;

                Save(CurrentSettings);
                SetCompilerSettings(CurrentSettings);
            }
        }

        #endregion
    }
}
