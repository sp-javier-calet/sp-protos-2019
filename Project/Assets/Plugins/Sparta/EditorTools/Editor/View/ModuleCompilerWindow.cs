using System.Collections.Generic;
using System.Text;
using SpartaTools.Editor.Build;
using SpartaTools.Editor.SpartaProject;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.View
{
    public class ModuleCompilerWindow : EditorWindow
    {
        #region Editor options

        [MenuItem("Sparta/Validate/Sparta compiler...", false, 103)]
        public static void CompileModule()
        {
            EditorWindow.GetWindow(typeof(ModuleCompilerWindow), false, "Compiler", true);
        }

        #endregion

        Vector2 _scrollPosition;
        List<ModuleCategory> _categories;
        bool _enableTvOS;
        bool _enableStandalone;

        Variant _selectedVariant;
        float _lastSelectionTime;

        // Depends on the order to sort colors in the UI
        enum CompileStatus
        {
            None = 0,
            NoAction = 1,
            Success = 2,
            HasWarnings = 3,
            Failed = 4,
            NotCompiled = 5
        }

        class Variant
        {
            public Module Module { get; private set; }

            public string Name;
            public CompileStatus Status;
            public BuildTarget Target;
            public bool IsEditorBuild;
            public string Log;

            public Variant(string name, Module module, BuildTarget target, bool editorBuild)
            {
                Name = name;
                Module = module;
                Target = target;
                IsEditorBuild = editorBuild;
                Log = string.Empty;
                Status = CompileStatus.NotCompiled;
            }
        }

        class ModuleData
        {
            public bool Show;

            public Module Module { get; private set; }

            public List<Variant> Variants { get; private set; }

            public CompileStatus Status
            {
                get
                {
                    var status = CompileStatus.None;
                    foreach(var variant in Variants)
                    {
                        if(variant.Status > status)
                        {
                            status = variant.Status;
                        }
                    }
                    return status;
                }
            }

            public ModuleData(Module module, bool compileTvOS, bool compileStandalone)
            {
                Module = module;
                Variants = new List<Variant>();

                Variants.Add(new Variant("Android", module, BuildTarget.Android, false));
                Variants.Add(new Variant("Android-Editor", module, BuildTarget.Android, true));
                Variants.Add(new Variant("iOS", module, BuildTarget.iOS, false));
                Variants.Add(new Variant("iOS-Editor", module, BuildTarget.iOS, true));

                if(compileTvOS)
                {
                    Variants.Add(new Variant("tvOS", module, BuildTarget.tvOS, false));
                    Variants.Add(new Variant("tvOS-Editor", module, BuildTarget.tvOS, true));
                }

                if(compileStandalone)
                {
                    Variants.Add(new Variant("macOS", module, BuildTarget.StandaloneOSX, false));
                    Variants.Add(new Variant("macOS-Editor", module, BuildTarget.StandaloneOSX, true));
                    Variants.Add(new Variant("linux", module, BuildTarget.StandaloneLinuxUniversal, false));
                    Variants.Add(new Variant("linux-Editor", module, BuildTarget.StandaloneLinuxUniversal, true));
                    Variants.Add(new Variant("Win Standalone", module, BuildTarget.StandaloneWindows, false));
                    Variants.Add(new Variant("Win-Editor", module, BuildTarget.StandaloneWindows, true));
                }
            }
        }

        class ModuleCategory
        {
            public string Name;
            public bool Show;
            public IList<ModuleData> Modules;

            public CompileStatus Status
            {
                get
                {
                    var status = CompileStatus.None;
                    foreach(var module in Modules)
                    {
                        var modStatus = module.Status;
                        if(modStatus > status)
                        {
                            status = modStatus;
                        }
                    }
                    return status;
                }
            }

            public ModuleCategory(string name)
            {
                Name = name;
                Show = true;
                Modules = new List<ModuleData>();
            }
        }



        List<ModuleCategory> LoadData()
        {
            var dic = new Dictionary<string, ModuleCategory>();
            var categories = new List<ModuleCategory>();

            var projectModules = Sparta.Current.GetModules();
            foreach(var module in projectModules.Values)
            {
                var categoryName = module.Type.ToString();

                ModuleCategory category;
                if(!dic.TryGetValue(categoryName, out category))
                {
                    category = new ModuleCategory(categoryName);
                    categories.Add(category);
                    dic.Add(categoryName, category);
                }

                category.Modules.Add(new ModuleData(module, _enableTvOS, _enableStandalone));
            }

            return categories;
        }

        void CompileFailed()
        {
            foreach(var category in _categories)
            {
                foreach(var data in category.Modules)
                {
                    foreach(var variant in data.Variants)
                    {
                        if(variant.Status == CompileStatus.Failed)
                        {
                            CompileVariant(variant, true);
                        }
                    }
                }
            }

            Repaint();
        }

        void CompileAll(bool onlyFullCategory = false)
        {
            ModuleCompiler.ClearOutputPath();

            foreach(var category in _categories)
            {
                foreach(var data in category.Modules)
                {
                    if(onlyFullCategory && (data.Module.Type != Module.ModuleType.Full))
                    {
                        continue;
                    }

                    foreach(var variant in data.Variants)
                    {
                        CompileVariant(variant, true);
                    }
                }
            }

            Repaint();
        }

        void CompileVariant(Variant variant, bool skipAutomatically)
        {
            try
            {
                var result = ModuleCompiler.Compile(variant.Module, variant.Target, variant.IsEditorBuild);
                variant.Status = result.Success ? CompileStatus.Success : CompileStatus.HasWarnings;
                variant.Log = result.Log;
            }
            catch(EmptyModuleException e)
            {
                variant.Status = CompileStatus.NoAction;
                variant.Log = e.ToString();
            }
            catch(DependencyNotFoundException e)
            {
                if(!skipAutomatically)
                {
                    var dialogMessage = string.Format("Failed while compiling '{0} - {1}'. Library file '{2}' not found. Make sure that dependencies and core libraries are compiled first.", variant.Module.Name, variant.Name, e.Dependency);
                    EditorUtility.DisplayDialog("Missing module dependency", dialogMessage, "Skip");
                }
                variant.Status = CompileStatus.Failed;
                variant.Log = e.ToString();
            }
            catch(SpartaBuildException e)
            {
                variant.Status = CompileStatus.Failed;
                variant.Log = e.ToString();
            }

            Repaint();
        }

        #region GUI

        void OnFocus()
        {
            Sparta.SetIcon(this, "Compiler", "Sparta Module compiler");
        }

        static void GUIShowLog(Variant variant)
        {
            var log = variant.Log;
            if(!string.IsNullOrEmpty(log))
            {
                switch(variant.Status)
                {
                case CompileStatus.Failed:
                    Debug.LogError(log);
                    break;
                case CompileStatus.HasWarnings:
                    Debug.LogWarning(log);
                    break;
                default:
                    Debug.Log(log);
                    break;
                }
            }
        }

        void GUIModuleVariant(Variant variant)
        {
            if(GUILayout.Button(new GUIContent(variant.Name, string.Format("{0} Module for {1}. {2}", variant.Module.Name, variant.Name, variant.Status)), GetLabelStyle(variant.Status)))
            {
                var t = Time.realtimeSinceStartup;
                if(variant == _selectedVariant && t - _lastSelectionTime < 0.2f)
                {
                    EditorUtility.DisplayProgressBar("Compile module", string.Format("Compiling {0} for {1}", variant.Module.Name, variant.Name), 0.1f);
                    try
                    {
                        CompileVariant(variant, false);
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }

                _lastSelectionTime = t;
                _selectedVariant = variant;
                Sparta.SelectedModule = variant.Module;
                GUIShowLog(variant);
            }
        }

        void GUIModule(ModuleData data)
        {
            data.Show = EditorGUILayout.Foldout(data.Show, 
                new GUIContent(data.Module.Name, string.Format("{0}.\n{1} module.\n{2}", data.Module.Description, data.Module.Type, data.Module.RelativePath)),
                GetFoldoutStyle(data.Status));
            if(data.Show)
            {
                // Show dependencies
                bool first = true;
                var builder = new StringBuilder();
                if(data.Module.Dependencies.Count > 0)
                {
                    GUILayout.BeginVertical(Styles.Group);
                    foreach(var dependency in data.Module.Dependencies)
                    {
                        if(first)
                        {
                            builder.Append(dependency);
                            first = false;
                        }
                        else
                        {
                            builder.AppendFormat("\n{0}", dependency);
                        }
                    }
                    EditorGUILayout.HelpBox(builder.ToString(), MessageType.None);
                    GUILayout.EndVertical();
                }

                EditorGUILayout.BeginVertical(Styles.Group);
                foreach(var variant in data.Variants)
                {
                    GUIModuleVariant(variant);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        }

        void GUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if(GUILayout.Button("Reload", EditorStyles.toolbarButton))
            {
                _categories = LoadData();
            }

            var tvOS = GUILayout.Toggle(_enableTvOS, new GUIContent("tvOS", "Compile modules for tvOS"), EditorStyles.toolbarButton);
            if(tvOS != _enableTvOS)
            {
                _enableTvOS = tvOS;
                _categories = LoadData();
            }

            var standalone = GUILayout.Toggle(_enableStandalone, new GUIContent("Standalone", "Compile module for Standalone platforms (MacOS, Win, Linux)"), EditorStyles.toolbarButton);
            if(standalone != _enableStandalone)
            {
                _enableStandalone = standalone;
                _categories = LoadData();
            }


            GUILayout.FlexibleSpace();

            if(GUILayout.Button("Compile Sparta Full only", EditorStyles.toolbarButton))
            {
                EditorUtility.DisplayProgressBar("Compile Sparta Full only", "Compiling all variants", 0.1f);
                try
                {
                    CompileAll(true);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            if(GUILayout.Button("Compile All", EditorStyles.toolbarButton))
            {
                EditorUtility.DisplayProgressBar("Compile All", "Compiling all modules and variants", 0.1f);
                try
                {
                    CompileAll();
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            if(GUILayout.Button("Compile Failed", EditorStyles.toolbarButton))
            {
                EditorUtility.DisplayProgressBar("Compile Failed", "Compiling failed modules and variants", 0.1f);
                try
                {
                    CompileFailed();
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            if(GUILayout.Button("Clean", EditorStyles.toolbarButton))
            {
                ModuleCompiler.ClearOutputPath();
                EditorUtility.DisplayDialog("Clean", "Output path cleaned successfully", "Accept");
            }

            GUILayout.EndHorizontal();
        }

        void OnGUI()
        {
            if(_categories == null)
            {
                _categories = LoadData();
            }

            GUIToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Label("Project modules", EditorStyles.boldLabel);

            foreach(var category in _categories)
            {
                category.Show = EditorGUILayout.Foldout(category.Show, new GUIContent(category.Name, string.Format("{0} Modules", category.Name)), GetFoldoutStyle(category.Status));
                if(category.Show)
                {
                    GUILayout.BeginVertical(Styles.Group);
                    foreach(var moduleData in category.Modules)
                    {
                        GUIModule(moduleData);
                    }
                    GUILayout.EndVertical();
                }
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }

        static GUIStyle GetFoldoutStyle(CompileStatus status)
        {
            var style = EditorStyles.foldout;

            switch(status)
            {
            case CompileStatus.NoAction:
                style = Styles.GrayFoldout;
                break;

            case CompileStatus.HasWarnings:
                style = Styles.YellowFoldout;
                break;

            case CompileStatus.Success:
                style = Styles.GreenFoldout;
                break;

            case CompileStatus.Failed:
                style = Styles.RedFoldout;
                break;
            }

            return style;
        }

        static GUIStyle GetLabelStyle(CompileStatus status)
        {
            var style = EditorStyles.label;

            switch(status)
            {
            case CompileStatus.NoAction:
                style = Styles.GrayLabel;
                break;

            case CompileStatus.HasWarnings:
                style = Styles.YellowLabel;
                break;

            case CompileStatus.Success:
                style = Styles.GreenLabel;
                break;

            case CompileStatus.Failed:
                style = Styles.RedLabel;
                break;
            }

            return style;
        }

        #endregion
    }
}