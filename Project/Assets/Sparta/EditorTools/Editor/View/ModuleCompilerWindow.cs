using UnityEngine;
using UnityEditor;
using SpartaTools.Editor.Build;
using SpartaTools.Editor.SpartaProject;
using System.Collections.Generic;
using System.Text;


namespace SpartaTools.Editor.View
{
    public class ModuleCompilerWindow : EditorWindow
    {
        [MenuItem("Sparta/Build/Module compiler", false, 001)]
        public static void CompileModule()
        {
            EditorWindow.GetWindow(typeof(ModuleCompiler), false, "Sparta compiler", true);
        }

        Variant _selectedVariant;

        private class Variant
        {
            public enum CompileStatus
            {
                NotCompiled,
                NoAction,
                Success,
                HasWarnings,
                Failed
            }

            public Module Module { get; private set; }
            public string Name;
            public CompileStatus Status;
            public BuildTarget Target;
            public bool IsEditorBuild;
            public bool Show;
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

            public ModuleData(Module module)
            {
                Module = module;
                Variants = new List<Variant>();

                Variants.Add(new Variant("Android", module, BuildTarget.Android, false));
                Variants.Add(new Variant("Android-Editor", module, BuildTarget.Android, true));
                Variants.Add(new Variant("iOS", module, BuildTarget.iOS, false));
                Variants.Add(new Variant("iOS-Editor", module, BuildTarget.iOS, true));
            }
        }

        class ModuleCategory
        {
            public string Name;
            public bool Show;
            public IList<ModuleData> Modules;

            public ModuleCategory(string name)
            {
                Name = name;
                Show = true;
                Modules = new List<ModuleData>();
            }
        }

        Vector2 _scrollPosition;
        List<ModuleCategory> _categories;

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

                category.Modules.Add(new ModuleData(module));
            }

            return categories;
        }

        void GUIModuleVariant(Variant variant)
        {
            variant.Show = EditorGUILayout.Foldout(variant.Show, variant.Name);
            if(variant.Show)
            {
                EditorGUILayout.BeginVertical(Styles.Group);
                EditorGUILayout.LabelField("Status: " + variant.Status);
                if(!string.IsNullOrEmpty(variant.Log))
                {
                    EditorGUILayout.TextArea(variant.Log);
                }
                if(GUILayout.Button("Compile"))
                {
                    variant.Log = ModuleCompiler.Compile(variant.Module, variant.Target, variant.IsEditorBuild);
                    variant.Status = Variant.CompileStatus.NoAction;//FIXME use code and exceptions
                }
                EditorGUILayout.EndVertical();
            }
        }

        void GUIModule(ModuleData data)
        {
            data.Show = EditorGUILayout.Foldout(data.Show, data.Module.Name);
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
            
        void OnGUI()
        {
            if(_categories == null)
            {
                _categories = LoadData();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Label("Project modules", EditorStyles.boldLabel);

            foreach(var category in _categories)
            {
                category.Show = EditorGUILayout.Foldout(category.Show, string.Format("{0} Modules", category.Name));
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
    }
}