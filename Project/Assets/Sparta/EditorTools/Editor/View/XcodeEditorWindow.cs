using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SpartaTools.Editor.Build;
using SpartaTools.Editor.Build.XcodeEditor;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.View
{
    public class XcodeEditorWindow : EditorWindow
    {
        #region Editor options

        [MenuItem("Sparta/Validate/XcodeMods...", false, 104)]
        public static void ShowBuildSettings()
        {
            EditorWindow.GetWindow(typeof(XcodeEditorWindow), false, "Xcode Mods", true);
        }

        #endregion

        class ModData
        {
            public string Type;
            public string Content;
            public XcodeMod Source;

            public ModData(string type, string content, XcodeMod mod)
            {
                Type = type;
                Content = content;
                Source = mod;
            }
        }

        Dictionary<string, List<ModData>> _modsData;
        Vector2 _scrollPosition;
        BuildTarget _target = BuildTarget.iOS;
        List<string> _activeSchemes;
        bool _asEditor;
        bool _asJenkins;
        bool _asAppend;
        bool _useActiveSchemes = true;
        bool _debugScheme;
        bool _releaseScheme;
        bool _shippingScheme;

        static string[] Schemes
        {
            get
            {
                // XCodeModSchemes prefs are written by BuildSet.
                var customPrefixes = BuildSet.CurrentXcodeModSchemes;
                if(string.IsNullOrEmpty(customPrefixes))
                {
                    return new string[0];
                }
                else
                {
                    return customPrefixes.Split(new char[]{ ';' });
                }
            }
        }

        bool _editEnabled;

        bool EditEnabled
        {
            set
            {
                bool changed = _editEnabled != value;
                _editEnabled = value;
                if(changed)
                {
                    RefreshIcon();
                }
            }
            get
            {
                return _editEnabled;
            }
        }

        #region Editor GUI

        void OnFocus()
        {
            RefreshIcon();
        }

        void RefreshIcon()
        {
            Sparta.SetIcon(this, "Xcode Mods", "Sparta Xcode Mods tool", EditEnabled);
        }

        bool AddToogleWithReload(bool status, string text, string tooltip)
        {
            var previousValue = status;
            var newStatus = GUILayout.Toggle(status, new GUIContent(text, tooltip), EditorStyles.toolbarButton);
            if(previousValue != newStatus)
            {
                _modsData = null;
            }
            return newStatus;
        }

        void ApplyXcodeMods(bool forceReapply)
        {
            if(forceReapply)
            {
                var accepted = EditorUtility.DisplayDialog("Warning", "This will leave the selected Xcode project corrupted. Use it only for debugging the XcodeMods code", "Continue");

                if(!accepted)
                {
                    return;
                }
            }

            var lastProject = XcodePostprocess.LastProjectPath;
            var path = EditorUtility.OpenFolderPanel("Select Target Project", lastProject, lastProject);

            // Check for cancelled popup
            if(!string.IsNullOrEmpty(path))
            {
                XcodePostprocess.ApplyXcodeMods(_target, path, forceReapply);
            }
        }

        void GUIToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            var isIos = _target == BuildTarget.iOS;
            isIos = AddToogleWithReload(isIos, "iOS", "Use iOS Variant");
            isIos = !AddToogleWithReload(!isIos, "tvOS", "Use tvOS Variant");
            _target = isIos ? BuildTarget.iOS : BuildTarget.tvOS;

            EditorGUILayout.Space();

            _asEditor = AddToogleWithReload(_asEditor, "Editor", "Enable 'editor' scheme");
            _asEditor = !AddToogleWithReload(!_asEditor, "Jenkins", "Enable 'jenkins' scheme");
            _asJenkins = !_asEditor;

            EditorGUILayout.Space();

            _asAppend = AddToogleWithReload(_asAppend, "Append", "Enable 'append' scheme");

            EditorGUILayout.Space();

            _useActiveSchemes = AddToogleWithReload(_useActiveSchemes, "Use Active Schemes", "Use current active schemes, set by Build Sets");

            if(!_useActiveSchemes)
            {
                _debugScheme = AddToogleWithReload(_debugScheme, "Debug", "Enable 'debug' scheme");
                _releaseScheme = AddToogleWithReload(_releaseScheme, "Release", "Enable 'release' scheme");
                _shippingScheme = AddToogleWithReload(_shippingScheme, "Shipping", "Enable 'shipping' scheme");
            }

            GUILayout.FlexibleSpace();
                
            if(GUILayout.Button(new GUIContent("Reload", "Reload XcodeMods content from disk"), EditorStyles.toolbarButton))
            {
                _modsData = null;
            }

            EditorGUILayout.Space();

            if(GUILayout.Button(new GUIContent("Apply", "Applies XcodeMods to an existing Xcodeproj. Applies Active Schemes configuration."), EditorStyles.toolbarButton))
            {
                ApplyXcodeMods(false);
            }

            if(GUILayout.Button(new GUIContent("Force Re-apply", "Reapplies XcodeMods to an existing Xcodeproj. Applies Active Schemes configuration. \nImportant: The resulting Project can be corrupted. Use only for debugiing XcodeMods"), EditorStyles.toolbarButton))
            {
                ApplyXcodeMods(true);
            }

            EditEnabled = GUILayout.Toggle(EditEnabled, new GUIContent("Advanced Mode", "Enables edition mode for project file"), EditorStyles.toolbarButton);

            GUILayout.EndHorizontal();
        }

        void GUIActiveSchemes()
        {
            GUILayout.Label("Active Schemes", EditorStyles.boldLabel);

            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label(string.Join(", ", _activeSchemes.ToArray()));
            GUILayout.EndVertical();
        }

        void GUIActivePatterns()
        {
            GUILayout.Label("Search Patterns", EditorStyles.boldLabel);
            GUILayout.BeginVertical(Styles.Group);
            foreach(var sh in _activeSchemes)
            {
                GUILayout.Label(string.Format("{0}.*.spxcodemod", sh));
                GUILayout.Label(string.Format("{0}.{1}.*.spxcodemod", _target.ToString().ToLower(), sh));
            }

            GUILayout.EndVertical();
        }

        void GUICurrentMods()
        {
            GUILayout.Label("Current Mods", EditorStyles.boldLabel);

            GUILayout.BeginVertical(Styles.Group);
            foreach(var dataType in _modsData.Keys)
            {
                GUILayout.Label(dataType);
                GUILayout.BeginVertical(Styles.Group);

                foreach(var mod in _modsData[dataType])
                {
                    var name = "Invalid module";
                    if(mod.Source != null)
                    {
                        name = Path.GetFileName(mod.Source.FilePath);
                    }
                    GUILayout.Label(new GUIContent(mod.Content, name));
                }

                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        void OnGUI()
        {
            GUIToolBar();

            // Reload if needed
            if(_modsData == null)
            {
                _modsData = LoadMods();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUIActiveSchemes();
            EditorGUILayout.Space();

            GUIActivePatterns();
            EditorGUILayout.Space();

            GUICurrentMods();

            EditorGUILayout.EndScrollView();
        }

        Dictionary<string, List<ModData>> LoadMods()
        {
            var handler = new XcodeModsHandler();

            _activeSchemes = new List<string>();
            _activeSchemes.Add("base");

            if(_asEditor)
            {
                _activeSchemes.Add("editor");
            }

            if(_asJenkins)
            {
                _activeSchemes.Add("jenkins");
            }

            if(_asAppend)
            {
                _activeSchemes.Clear();
                _activeSchemes.Add("append");
            }

            if(_useActiveSchemes)
            {
                foreach(var sh in Schemes)
                {
                    _activeSchemes.Add(sh);
                }
            }
            else
            {
                if(_debugScheme)
                    _activeSchemes.Add("debug");
                if(_releaseScheme)
                    _activeSchemes.Add("release");
                if(_shippingScheme)
                    _activeSchemes.Add("shipping");
            }

            handler.Load(_target, _activeSchemes);
            return handler.Data;
        }

        /// <summary>
        /// Xcode mods handler.
        /// </summary>
        class XcodeModsHandler : XCodeProjectEditor, IComparer<ModData>
        {
            public Dictionary<string, List<ModData>> Data = new Dictionary<string, List<ModData>>();
            XcodeMod _currentXcodeMod;

            public void Load(BuildTarget target, List<string> schemes)
            {
                var mods = new XcodeModsSet(target);
                mods.AddScheme(schemes.ToArray());

                foreach(string file in mods.Files)
                {
                    try
                    {
                        _currentXcodeMod = new XcodeMod(file);
                        _currentXcodeMod.Apply(this);
                        _currentXcodeMod = null;
                    }
                    catch
                    {
                        Add(new ModData("Failed XcodeMod files", file, _currentXcodeMod));
                    }
                }

                foreach(var list in Data.Values)
                {
                    list.Sort(this);
                }
            }

            #region IComparer implementation

            int IComparer<ModData>.Compare(ModData x, ModData y)
            {
                return x.Content.CompareTo(y.Content);
            }

            #endregion

            void Add(ModData data)
            {
                List<ModData> list;
                if(!Data.TryGetValue(data.Type, out list))
                {
                    list = new List<ModData>();
                    Data.Add(data.Type, list);
                }
                list.Add(data);
            }

            public void AddHeaderSearchPath(string path)
            {
                Add(new ModData("Header Path", path, _currentXcodeMod));
            }

            public void AddLibrarySearchPath(string path)
            {
                Add(new ModData("Library Path", path, _currentXcodeMod));
            }

            public void CopyFile(string basePath, string src, string dst)
            {
                Add(new ModData("Copy File", string.Format("{0} > {1}", src, dst), _currentXcodeMod));
            }

            public void AddFile(string path)
            {
                Add(new ModData("File", path, _currentXcodeMod));
            }

            public void AddFile(string path, string[] flags)
            {
                Add(new ModData("File", string.Format("{0} [{1}]", path, string.Join(",", flags)), _currentXcodeMod));
            }

            public void AddFolder(string path)
            {
                Add(new ModData("Folder", path, _currentXcodeMod));
            }

            public void AddLibrary(string path, bool weak)
            {
                Add(new ModData("Library", string.Format("{0} : {1}", path, weak ? "weak" : "required"), _currentXcodeMod));
            }

            public void AddFramework(string framework, bool weak)
            {
                Add(new ModData("Framework", string.Format("{0} : {1}", framework, weak ? "weak" : "required"), _currentXcodeMod));
            }

            public void SetBuildSetting(string name, string value)
            {
                Add(new ModData("Build Setting", string.Format("{0} = {1}", name, value), _currentXcodeMod));
            }

            public void AddLocalization(string lang)
            {
                Add(new ModData("Localization", string.Format("Language '{0}'", lang), _currentXcodeMod));
            }

            public void AddLocalization(string name, string path)
            {
                Add(new ModData("Localization", string.Format("{0} > {1}", name, path), _currentXcodeMod));
            }

            public void AddLocalization(string name, string path, string variantGroup)
            {
                Add(new ModData("Localization", string.Format("{0}/{1} > {2}", variantGroup, name, path), _currentXcodeMod));
            }

            public void AddPlistFields(IDictionary data)
            {
                Add(new ModData("Info Plist", XMiniJSON.jsonEncode(data), _currentXcodeMod));
            }

            public void AddShellScript(string script, string shell)
            {
                Add(new ModData("Shell Script", string.Format("{0} ({1})", script, shell), _currentXcodeMod));
            }

            public void AddShellScript(string script, string shell, int order)
            {
                Add(new ModData("Shell Script", string.Format("{0} ({1}) at position {2}. ", script, shell, order), _currentXcodeMod));
            }

            public void AddShellScript(string script, string shell, string target, int order)
            {
                Add(new ModData("Shell Script", string.Format("{0} ({1}) at position {2}. Target '{3}'.", script, shell, order, target), _currentXcodeMod));
            }

            public void SetSystemCapability(string name, bool enabled)
            {
                Add(new ModData("System Capability", string.Format("{0} = {1}", name, enabled), _currentXcodeMod));
            }

            public void SetProvisioningProfile(string path)
            {
                Add(new ModData("Provisioning Profile", path, _currentXcodeMod));
            }

            public void AddKeychainAccessGroup(string entitlementsFile, string accessGroup)
            {
                Add(new ModData("Keychain Access Group", string.Format("{0} in {1}", accessGroup, entitlementsFile), _currentXcodeMod));
            }

            public void AddKeychainAccessGroup(string accessGroup)
            {
                Add(new ModData("Keychain Access Group", string.Format("{0} in default entitlements file", accessGroup), _currentXcodeMod));
            }

            public void AddPushNotificationsEntitlement(bool isProduction)
            {
                var envName = string.Empty;
                if(isProduction)
                {
                    envName = "production";
                }
                else
                {
                    envName = "development";
                }

                Add(new ModData("Push Notifications Environment set to", string.Format("{0} in default entitlements file", envName), _currentXcodeMod));
            }

            public void AddPushNotificationsEntitlement(string entitlementsFile, bool isProduction)
            {
                var envName = string.Empty;
                if(isProduction)
                {
                    envName = "production";
                }
                else
                {
                    envName = "development";
                }
                Add(new ModData("Push Notifications Environment set to", string.Format("{0} in {1}", envName, entitlementsFile), _currentXcodeMod));
            }

            public void Commit()
            {
            }
        }

        #endregion
    }
}

