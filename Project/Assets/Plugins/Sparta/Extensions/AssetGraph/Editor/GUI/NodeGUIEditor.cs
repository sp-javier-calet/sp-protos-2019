using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace AssetBundleGraph
{
    /**
		GUI Inspector to NodeGUI (Through NodeGUIInspectorHelper)
	*/
    [CustomEditor(typeof(NodeGUIInspectorHelper))]
    public class NodeGUIEditor : Editor
    {

        public static BuildTargetGroup currentEditingGroup =
            BuildTargetUtility.DefaultTarget;

        [NonSerialized]
        private IModifier m_modifier;
        [NonSerialized]
        private IValidator m_validator;
        [NonSerialized]
        private IPrefabBuilder m_prefabBuilder;

        private Type selectedType;

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private void DoInspectorLoaderGUI(NodeGUI node)
        {
            if(node.Data.LoaderLoadPath == null)
            {
                return;
            }

            EditorGUILayout.HelpBox("Loader: Load assets in given directory path.", MessageType.Info);
            UpdateNodeName(node);

            bool newPreProcess = EditorGUILayout.Toggle("Pre-Processing", node.Data.PreProcess);

            if(newPreProcess != node.Data.PreProcess)
            {
                using(new RecordUndoScope("PreProcess Changed", node, true))
                {
                    node.Data.PreProcess = newPreProcess;
                }
            }

            bool newPermanent = EditorGUILayout.Toggle("Permanent Processing", node.Data.Permanent);
            if(newPermanent != node.Data.Permanent)
            {
                using(new RecordUndoScope("Permanent Changed", node, true))
                {
                    node.Data.Permanent = newPermanent;
                }
            }

            GUILayout.Space(10f);

            //Show target configuration tab
            DrawPlatformSelector(node);
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = DrawOverrideTargetToggle(node, node.Data.LoaderLoadPath.ContainsValueOf(currentEditingGroup), (bool b) =>
                {
                    using(new RecordUndoScope("Remove Target Load Path Settings", node, true))
                    {
                        if(b)
                        {
                            node.Data.LoaderLoadPath[currentEditingGroup] = node.Data.LoaderLoadPath.DefaultValue;
                        }
                        else
                        {
                            node.Data.LoaderLoadPath.Remove(currentEditingGroup);
                        }
                    }
                });

                using(disabledScope)
                {
                    EditorGUILayout.LabelField("Load Path:");
                    var newLoadPath = EditorGUILayout.TextField(
                        SystemDataUtility.GetProjectName() + AssetBundleGraphSettings.ASSETS_PATH,
                        node.Data.LoaderLoadPath[currentEditingGroup]
                    );
                    if(newLoadPath != node.Data.LoaderLoadPath[currentEditingGroup])
                    {
                        using(new RecordUndoScope("Load Path Changed", node, true))
                        {
                            node.Data.LoaderLoadPath[currentEditingGroup] = newLoadPath;
                        }
                    }
                }
            }

            EditorGUILayout.Space();
            if(GUILayout.Button("Select Tree"))
            {
                AssetBundleGraphEditorWindow.SelectAllRelatedTree(new string[] { node.Id }, false);
            }

            if(!string.IsNullOrEmpty(node.Data.LoaderLoadPath.CurrentPlatformValue) && GUILayout.Button("Select Folder in Project"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/" + node.Data.LoaderLoadPath.CurrentPlatformValue);
            }

        }

        private void DoInspectorFilterGUI(NodeGUI node)
        {
            EditorGUILayout.HelpBox("Filter: Filter incoming assets by keywords and types. You can use regular expressions for keyword field.", MessageType.Info);
            UpdateNodeName(node);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label("Filter Settings:");
                FilterEntry removing = null;
                for(int i = 0; i < node.Data.FilterConditions.Count; ++i)
                {
                    var cond = node.Data.FilterConditions[i];

                    Action messageAction = null;

                    using(new GUILayout.HorizontalScope())
                    {
                        if(GUILayout.Button("-", GUILayout.Width(30)))
                        {
                            removing = cond;
                        }
                        else
                        {
                            var newName = cond.Name;
                            var newContainsKeyword = cond.FilterKeyword;
                            bool newIsExclusion = cond.IsExclusion;

                            GUIStyle s = new GUIStyle((GUIStyle)"TextFieldDropDownText");

                            using(new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField("Name:", GUILayout.MaxWidth(40));
                                newName = EditorGUILayout.TextField(cond.Name, s, GUILayout.MaxWidth(200));

                                EditorGUILayout.LabelField("Filter:", GUILayout.MaxWidth(40));
                                newContainsKeyword = EditorGUILayout.TextField(cond.FilterKeyword, s, GUILayout.MaxWidth(200));
                                newIsExclusion = GUILayout.Toggle(cond.IsExclusion, " Negated", GUILayout.MaxWidth(80));
                                if(GUILayout.Button(cond.FilterKeytype, "Popup", GUILayout.MinWidth(220)))
                                {
                                    var ind = i;// need this because of closure locality bug in unity C#
                                    NodeGUI.ShowFilterKeyTypeMenu(
                                        cond.FilterKeytype,
                                        (string selectedTypeStr) =>
                                        {
                                            using(new RecordUndoScope("Modify Filter Type", node, true))
                                            {
                                                node.Data.FilterConditions[ind].FilterKeytype = selectedTypeStr;
                                            }
                                        }
                                    );
                                }

                            }
                            if(newName != cond.Name)
                            {
                                using(new RecordUndoScope("Modify Filter Name", node, true))
                                {
                                    cond.Name = newName;
                                    // event must raise to propagate change to connection associated with point
                                    NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_LABELCHANGED, node, Vector2.zero, cond.ConnectionPoint));
                                }
                            }
                            if(newContainsKeyword != cond.FilterKeyword)
                            {
                                using(new RecordUndoScope("Modify Filter Keyword", node, true))
                                {
                                    cond.FilterKeyword = newContainsKeyword;
                                    // event must raise to propagate change to connection associated with point
                                    NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_LABELCHANGED, node, Vector2.zero, cond.ConnectionPoint));
                                }
                            }
                            if(newIsExclusion != cond.IsExclusion)
                            {
                                using(new RecordUndoScope("Modify Filter Exclusion", node, true))
                                {
                                    cond.IsExclusion = newIsExclusion;
                                    // event must raise to propagate change to connection associated with point
                                    NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_LABELCHANGED, node, Vector2.zero, cond.ConnectionPoint));
                                }
                            }
                        }
                    }

                    if(messageAction != null)
                    {
                        using(new GUILayout.HorizontalScope())
                        {
                            messageAction.Invoke();
                        }
                    }
                }

                // add contains keyword interface.
                if(GUILayout.Button("+"))
                {
                    using(new RecordUndoScope("Add Filter Condition", node, true))
                    {
                        node.Data.AddFilterCondition(
                            AssetBundleGraphSettings.DEFAULT_FILTER_NAME,
                            AssetBundleGraphSettings.DEFAULT_FILTER_KEYWORD,
                            AssetBundleGraphSettings.DEFAULT_FILTER_KEYTYPE,
                            AssetBundleGraphSettings.DEFAULT_FILTER_EXCLUSION
                            );
                        NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_ADDED));
                    }
                }

                if(removing != null)
                {
                    using(new RecordUndoScope("Remove Filter Condition", node, true))
                    {
                        // event must raise to remove connection associated with point
                        NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_DELETED, node, Vector2.zero, removing.ConnectionPoint));
                        node.Data.RemoveFilterCondition(removing);
                    }
                }
            }
        }

        private void DoInspectorImportSettingGUI(NodeGUI node)
        {
            EditorGUILayout.HelpBox("ImportSetting: Force apply import settings to given assets.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            /*
				importer node has no platform key. 
				platform key is contained by Unity's importer inspector itself.
			*/
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                Type incomingType = FindIncomingAssetType(node.Data.InputPoints[0]);
                IntegratedGUIImportSetting.ConfigStatus status =
                    IntegratedGUIImportSetting.GetConfigStatus(node.Data);

                if(incomingType == null)
                {
                    // try to retrieve incoming type from configuration
                    if(status == IntegratedGUIImportSetting.ConfigStatus.GoodSampleFound)
                    {
                        incomingType = IntegratedGUIImportSetting.GetReferenceAssetImporter(node.Data.Id).GetType();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("ImportSetting needs a single type of incoming assets.", MessageType.Info);


                        if(GUILayout.Button("Select Type", "Popup", GUILayout.MinWidth(220)))
                        {
                            NodeGUI.ShowImportSettingsKeyTypeMenu(
                                "Select Type",
                                (Type selectedTypeStr) =>
                                {
                                    IntegratedGUIImportSetting.SaveSampleFile(node.Data, selectedTypeStr);
                                    status = IntegratedGUIImportSetting.ConfigStatus.GoodSampleFound;
                                }
                            );
                        }

                        return;
                    }
                }

                switch(status)
                {
                case IntegratedGUIImportSetting.ConfigStatus.NoSampleFound:
                    // IntegratedGUIImportSetting.Setup() must run to grab another sample to configure.
                    EditorGUILayout.HelpBox("Press Refresh to configure.", MessageType.Info);
                    break;
                case IntegratedGUIImportSetting.ConfigStatus.GoodSampleFound:
                    if(GUILayout.Button("Configure Import Setting"))
                    {
#if UNITY_5_4_OR_NEWER
                        Selection.activeObject = IntegratedGUIImportSetting.GetReferenceAssetImporter(node.Data.Id);
#else
                            Selection.activeObject = IntegratedGUIImportSetting.GetReferenceAsset(node.Data.Id);
#endif
                    }
                    if(GUILayout.Button("Reset Import Setting"))
                    {
                        IntegratedGUIImportSetting.ResetConfig(node.Data);
                    }
                    break;
                case IntegratedGUIImportSetting.ConfigStatus.TooManySamplesFound:
                    if(GUILayout.Button("Reset Import Setting"))
                    {
                        IntegratedGUIImportSetting.ResetConfig(node.Data);
                    }
                    break;
                }
            }
        }

        private void DoInspectorModifierGUI(NodeGUI node)
        {
            EditorGUILayout.HelpBox("Modifier: Modify asset settings.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                Type incomingType = FindIncomingAssetType(node.Data.InputPoints[0]);

                if(incomingType == null)
                {
                    // if there is no asset input to determine incomingType,
                    // retrieve from assigned Modifier.
                    incomingType = ModifierUtility.GetModifierTargetType(node.Data.ScriptClassName);

                    if(incomingType == null)
                    {
                        var selected = selectedType == null ? "Select Type" : selectedType.ToString();
                        if(GUILayout.Button(selected, "Popup", GUILayout.MinWidth(220)))
                        {
                            NodeGUI.ShowKeyTypeMenu(selected, x => selectedType = x);
                        }
                        EditorGUILayout.Space();

                        incomingType = selectedType;

                        if(incomingType == null)
                        {
                            EditorGUILayout.HelpBox("Modifier needs a single type of incoming assets.", MessageType.Info);
                            return;
                        }
                    }
                }

                var map = ModifierUtility.GetAttributeClassNameMap(incomingType);
                if(map.Count > 0)
                {
                    using(new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Modifier");
                        var guiName = ModifierUtility.GetModifierGUIName(node.Data.ScriptClassName);
                        if(GUILayout.Button(guiName, "Popup", GUILayout.MinWidth(150f)))
                        {
                            var builders = map.Keys.ToList();

                            if(builders.Count > 0)
                            {
                                NodeGUI.ShowTypeNamesMenu(guiName, builders, (string selectedGUIName) =>
                                    {
                                        using(new RecordUndoScope("Change Modifier class", node, true))
                                        {
                                            m_modifier = ModifierUtility.CreateModifier(selectedGUIName, incomingType);
                                            if(m_modifier != null)
                                            {
                                                node.Data.ScriptClassName = ModifierUtility.GUINameToClassName(selectedGUIName, incomingType);
                                                node.Data.InstanceData[currentEditingGroup] = m_modifier.Serialize();
                                            }
                                        }
                                    }
                                );
                            }
                            else
                            {
                                return;
                            }
                        }
                    }

                }
                else
                {
                    string[] menuNames = AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_MODIFIER.Split('/');
                    EditorGUILayout.HelpBox(
                        string.Format(
                            "No CustomModifier found for {3} type. \n" +
                            "You need to create at least one Modifier script to select script for Modifier. " +
                            "To start, select {0}>{1}>{2} menu and create a new script.",
                            menuNames[1], menuNames[2], menuNames[3], incomingType.FullName
                        ), MessageType.Info);
                    return;
                }

                GUILayout.Space(10f);

                if(DrawPlatformSelector(node))
                {
                    // if platform tab is changed, renew modifierModifierInstance for that tab.
                    m_modifier = null;
                }
                using(new EditorGUILayout.VerticalScope())
                {
                    var disabledScope = DrawOverrideTargetToggle(node, node.Data.InstanceData.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                    {
                        using(new RecordUndoScope("Override Target Modifier", node, true))
                        {
                            if(enabled)
                            {
                                node.Data.InstanceData[currentEditingGroup] = node.Data.InstanceData.DefaultValue;
                            }
                            else
                            {
                                node.Data.InstanceData.Remove(currentEditingGroup);
                            }
                            m_modifier = null;
                        }
                    });

                    using(disabledScope)
                    {
                        //reload modifierModifier instance from saved modifierModifier data.
                        if(m_modifier == null)
                        {
                            m_modifier = ModifierUtility.CreateModifier(node.Data, currentEditingGroup);
                            if(m_modifier != null)
                            {
                                node.Data.ScriptClassName = m_modifier.GetType().FullName;
                                if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                {
                                    node.Data.InstanceData[currentEditingGroup] = m_modifier.Serialize();
                                }
                            }
                        }

                        if(m_modifier != null)
                        {
                            Action onChangedAction = () =>
                            {
                                using(new RecordUndoScope("Change Modifier Setting", node, true))
                                {
                                    node.Data.ScriptClassName = m_modifier.GetType().FullName;
                                    if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                    {
                                        node.Data.InstanceData[currentEditingGroup] = m_modifier.Serialize();
                                    }
                                }
                            };

                            m_modifier.OnInspectorGUI(onChangedAction);
                        }
                    }
                }
            }
        }

        private void DoInspectorGroupingGUI(NodeGUI node)
        {
            if(node.Data.GroupingKeywords == null)
            {
                return;
            }

            EditorGUILayout.HelpBox("Grouping: Create group of assets.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            //Show target configuration tab
            DrawPlatformSelector(node);
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = DrawOverrideTargetToggle(node, node.Data.GroupingKeywords.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                {
                    using(new RecordUndoScope("Remove Target Grouping Keyword Settings", node, true))
                    {
                        if(enabled)
                        {
                            node.Data.GroupingKeywords[currentEditingGroup] = node.Data.GroupingKeywords.DefaultValue;
                        }
                        else
                        {
                            node.Data.GroupingKeywords.Remove(currentEditingGroup);
                        }
                    }
                });

                using(disabledScope)
                {
                    var newGroupingKeyword = EditorGUILayout.TextField("Grouping Keyword", node.Data.GroupingKeywords[currentEditingGroup]);
                    EditorGUILayout.HelpBox(
                        "Grouping Keyword requires \"*\" in itself. It assumes there is a pattern such as \"ID_0\" in incoming paths when configured as \"ID_*\" ",
                        MessageType.Info);

                    if(newGroupingKeyword != node.Data.GroupingKeywords[currentEditingGroup])
                    {
                        using(new RecordUndoScope("Change Grouping Keywords", node, true))
                        {
                            node.Data.GroupingKeywords[currentEditingGroup] = newGroupingKeyword;
                        }
                    }
                }
            }
        }

        private void DoInspectorPrefabBuilderGUI(NodeGUI node)
        {
            EditorGUILayout.HelpBox("PrefabBuilder: Create prefab with given assets and script.", MessageType.Info);
            UpdateNodeName(node);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                var map = PrefabBuilderUtility.GetAttributeClassNameMap();
                if(map.Count > 0)
                {
                    using(new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("PrefabBuilder");
                        var guiName = PrefabBuilderUtility.GetPrefabBuilderGUIName(node.Data.ScriptClassName);

                        if(GUILayout.Button(guiName, "Popup", GUILayout.MinWidth(150f)))
                        {
                            var builders = map.Keys.ToList();

                            if(builders.Count > 0)
                            {
                                NodeGUI.ShowTypeNamesMenu(guiName, builders, (string selectedGUIName) =>
                                    {
                                        using(new RecordUndoScope("Change PrefabBuilder class", node, true))
                                        {
                                            m_prefabBuilder = PrefabBuilderUtility.CreatePrefabBuilder(selectedGUIName);
                                            if(m_prefabBuilder != null)
                                            {
                                                node.Data.ScriptClassName = PrefabBuilderUtility.GUINameToClassName(selectedGUIName);
                                                node.Data.InstanceData.DefaultValue = m_prefabBuilder.Serialize();
                                            }
                                        }
                                    }
                                );
                            }
                        }
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(node.Data.ScriptClassName))
                    {
                        EditorGUILayout.HelpBox(
                            string.Format(
                                "Your PrefabBuilder script {0} is missing from assembly. Did you delete script?", node.Data.ScriptClassName), MessageType.Info);
                    }
                    else
                    {
                        string[] menuNames = AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_PREFABBUILDER.Split('/');
                        EditorGUILayout.HelpBox(
                            string.Format(
                                "You need to create at least one PrefabBuilder script to use PrefabBuilder node. To start, select {0}>{1}>{2} menu and create new script from template.",
                                menuNames[1], menuNames[2], menuNames[3]
                            ), MessageType.Info);
                    }
                }

                GUILayout.Space(10f);

                if(DrawPlatformSelector(node))
                {
                    // if platform tab is changed, renew prefabBuilder for that tab.
                    m_prefabBuilder = null;
                }
                using(new EditorGUILayout.VerticalScope())
                {
                    var disabledScope = DrawOverrideTargetToggle(node, node.Data.InstanceData.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                    {
                        using(new RecordUndoScope("Override Target PrefabBuilder", node, true))
                        {
                            if(enabled)
                            {
                                node.Data.InstanceData[currentEditingGroup] = node.Data.InstanceData.DefaultValue;
                            }
                            else
                            {
                                node.Data.InstanceData.Remove(currentEditingGroup);
                            }
                            m_prefabBuilder = null;
                        }
                    });

                    using(disabledScope)
                    {
                        //reload prefabBuilder instance from saved instance data.
                        if(m_prefabBuilder == null)
                        {
                            m_prefabBuilder = PrefabBuilderUtility.CreatePrefabBuilder(node.Data, currentEditingGroup);
                            if(m_prefabBuilder != null)
                            {
                                node.Data.ScriptClassName = m_prefabBuilder.GetType().FullName;
                                if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                {
                                    node.Data.InstanceData[currentEditingGroup] = m_prefabBuilder.Serialize();
                                }
                            }
                        }

                        if(m_prefabBuilder != null)
                        {
                            Action onChangedAction = () =>
                            {
                                using(new RecordUndoScope("Change PrefabBuilder Setting", node, true))
                                {
                                    node.Data.ScriptClassName = m_prefabBuilder.GetType().FullName;
                                    if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                    {
                                        node.Data.InstanceData[currentEditingGroup] = m_prefabBuilder.Serialize();
                                    }
                                }
                            };

                            m_prefabBuilder.OnInspectorGUI(onChangedAction);
                        }
                    }
                }
            }
        }

        private void DoInspectorBundleConfiguratorGUI(NodeGUI node)
        {
            if(node.Data.BundleNameTemplate == null) return;

            EditorGUILayout.HelpBox("BundleConfigurator: Create asset bundle settings with given group of assets.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                var newUseGroupAsVariantValue = GUILayout.Toggle(node.Data.BundleConfigUseGroupAsVariants, "Use input group as variants");
                if(newUseGroupAsVariantValue != node.Data.BundleConfigUseGroupAsVariants)
                {
                    using(new RecordUndoScope("Change Bundle Config", node, true))
                    {
                        node.Data.BundleConfigUseGroupAsVariants = newUseGroupAsVariantValue;

                        // TODO: preserve variants
                        List<Variant> rv = new List<Variant>(node.Data.Variants);
                        foreach(var v in rv)
                        {
                            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_DELETED, node, Vector2.zero, v.ConnectionPoint));
                            node.Data.RemoveVariant(v);
                        }
                    }
                }

                using(new EditorGUI.DisabledGroupScope(newUseGroupAsVariantValue))
                {
                    GUILayout.Label("Variants:");
                    var variantNames = node.Data.Variants.Select(v => v.Name).ToList();
                    Variant removing = null;
                    foreach(var v in node.Data.Variants)
                    {
                        using(new GUILayout.HorizontalScope())
                        {
                            if(GUILayout.Button("-", GUILayout.Width(30)))
                            {
                                removing = v;
                            }
                            else
                            {
                                GUIStyle s = new GUIStyle((GUIStyle)"TextFieldDropDownText");
                                Action makeStyleBold = () =>
                                {
                                    s.fontStyle = FontStyle.Bold;
                                    s.fontSize = 12;
                                };

                                IntegratedGUIBundleConfigurator.ValidateVariantName(v.Name, variantNames,
                                    makeStyleBold,
                                    makeStyleBold,
                                    makeStyleBold);

                                var variantName = EditorGUILayout.TextField(v.Name, s);

                                if(variantName != v.Name)
                                {
                                    using(new RecordUndoScope("Change Variant Name", node, true))
                                    {
                                        v.Name = variantName;
                                    }
                                }
                            }
                        }
                    }
                    if(GUILayout.Button("+"))
                    {
                        using(new RecordUndoScope("Add Variant", node, true))
                        {
                            node.Data.AddVariant(AssetBundleGraphSettings.BUNDLECONFIG_VARIANTNAME_DEFAULT);
                        }
                    }
                    if(removing != null)
                    {
                        using(new RecordUndoScope("Remove Variant", node, true))
                        {
                            // event must raise to remove connection associated with point
                            NodeGUIUtility.NodeEventHandler(new NodeEvent(NodeEvent.EventType.EVENT_CONNECTIONPOINT_DELETED, node, Vector2.zero, removing.ConnectionPoint));
                            node.Data.RemoveVariant(removing);
                        }
                    }
                }
            }

            //Show target configuration tab
            DrawPlatformSelector(node);
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = DrawOverrideTargetToggle(node, node.Data.BundleNameTemplate.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                {
                    using(new RecordUndoScope("Remove Target Bundle Name Template Setting", node, true))
                    {
                        if(enabled)
                        {
                            node.Data.BundleNameTemplate[currentEditingGroup] = node.Data.BundleNameTemplate.DefaultValue;
                        }
                        else
                        {
                            node.Data.BundleNameTemplate.Remove(currentEditingGroup);
                        }
                    }
                });

                using(disabledScope)
                {
                    var bundleNameTemplate = EditorGUILayout.TextField("Bundle Name Template", node.Data.BundleNameTemplate[currentEditingGroup]).ToLower();

                    if(bundleNameTemplate != node.Data.BundleNameTemplate[currentEditingGroup])
                    {
                        using(new RecordUndoScope("Change Bundle Name Template", node, true))
                        {
                            node.Data.BundleNameTemplate[currentEditingGroup] = bundleNameTemplate;
                        }
                    }
                }
            }
        }

        private void DoInspectorBundleBuilderGUI(NodeGUI node)
        {
            if(node.Data.BundleBuilderBundleOptions == null)
            {
                return;
            }

            EditorGUILayout.HelpBox("BundleBuilder: Build asset bundles with given asset bundle settings.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            //Show target configuration tab
            DrawPlatformSelector(node);
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = DrawOverrideTargetToggle(node, node.Data.BundleBuilderBundleOptions.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                {
                    using(new RecordUndoScope("Remove Target Bundle Options", node, true))
                    {
                        if(enabled)
                        {
                            node.Data.BundleBuilderBundleOptions[currentEditingGroup] = node.Data.BundleBuilderBundleOptions.DefaultValue;
                        }
                        else
                        {
                            node.Data.BundleBuilderBundleOptions.Remove(currentEditingGroup);
                        }
                    }
                });

                using(disabledScope)
                {
                    int bundleOptions = node.Data.BundleBuilderBundleOptions[currentEditingGroup];

                    bool isDisableWriteTypeTreeEnabled = 0 < (bundleOptions & (int)BuildAssetBundleOptions.DisableWriteTypeTree);
                    bool isIgnoreTypeTreeChangesEnabled = 0 < (bundleOptions & (int)BuildAssetBundleOptions.IgnoreTypeTreeChanges);

                    // buildOptions are validated during loading. Two flags should not be true at the same time.
                    UnityEngine.Assertions.Assert.IsFalse(isDisableWriteTypeTreeEnabled && isIgnoreTypeTreeChangesEnabled);

                    bool isSomethingDisabled = isDisableWriteTypeTreeEnabled || isIgnoreTypeTreeChangesEnabled;

                    foreach(var option in AssetBundleGraphSettings.BundleOptionSettings)
                    {

                        // contains keyword == enabled. if not, disabled.
                        bool isEnabled = (bundleOptions & (int)option.option) != 0;

                        bool isToggleDisabled =
                            (option.option == BuildAssetBundleOptions.DisableWriteTypeTree && isIgnoreTypeTreeChangesEnabled) ||
                            (option.option == BuildAssetBundleOptions.IgnoreTypeTreeChanges && isDisableWriteTypeTreeEnabled);

                        using(new EditorGUI.DisabledGroupScope(isToggleDisabled))
                        {
                            var result = EditorGUILayout.ToggleLeft(option.description, isEnabled);
                            if(result != isEnabled)
                            {
                                using(new RecordUndoScope("Change Bundle Options", node, true))
                                {
                                    bundleOptions = (result) ?
                                        ((int)option.option | bundleOptions) :
                                        (((~(int)option.option)) & bundleOptions);
                                    node.Data.BundleBuilderBundleOptions[currentEditingGroup] = bundleOptions;
                                }
                            }
                        }
                    }
                    if(isSomethingDisabled)
                    {
                        EditorGUILayout.HelpBox("'Disable Write Type Tree' and 'Ignore Type Tree Changes' can not be used together.", MessageType.Info);
                    }
                }
            }
        }


        private void DoInspectorExporterGUI(NodeGUI node)
        {
            if(node.Data.ExporterExportPath == null)
            {
                return;
            }

            EditorGUILayout.HelpBox("Exporter: Export given files to output directory.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            //Show target configuration tab
            DrawPlatformSelector(node);
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = DrawOverrideTargetToggle(node, node.Data.ExporterExportPath.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                {
                    using(new RecordUndoScope("Remove Target Export Settings", node, true))
                    {
                        if(enabled)
                        {
                            node.Data.ExporterExportPath[currentEditingGroup] = node.Data.ExporterExportPath.DefaultValue;
                        }
                        else
                        {
                            node.Data.ExporterExportPath.Remove(currentEditingGroup);
                        }
                    }
                });

                using(disabledScope)
                {
                    ExporterExportOption opt = (ExporterExportOption)node.Data.ExporterExportOption[currentEditingGroup];
                    var newOption = (ExporterExportOption)EditorGUILayout.EnumPopup("Export Option", opt);
                    if(newOption != opt)
                    {
                        using(new RecordUndoScope("Change Export Option", node, true))
                        {
                            node.Data.ExporterExportOption[currentEditingGroup] = (int)newOption;
                        }
                    }

                    EditorGUILayout.LabelField("Export Path:");
                    var newExportPath = EditorGUILayout.TextField(
                        SystemDataUtility.GetProjectName(),
                        node.Data.ExporterExportPath[currentEditingGroup]
                    );

                    var exporterNodePath = FileUtility.GetPathWithProjectPath(newExportPath);
                    if(IntegratedGUIExporter.ValidateExportPath(
                        newExportPath,
                        exporterNodePath,
                        () =>
                        {
                            // TODO Make text field bold
                        },
                        () =>
                        {
                            using(new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(exporterNodePath + " does not exist.");
                                if(GUILayout.Button("Create directory"))
                                {
                                    using(new SaveScope(node))
                                    {
                                        Directory.CreateDirectory(exporterNodePath);
                                    }
                                }
                            }
                            EditorGUILayout.Space();

                            EditorGUILayout.LabelField("Available Directories:");
                            string[] dirs = Directory.GetDirectories(Path.GetDirectoryName(exporterNodePath));
                            foreach(string s in dirs)
                            {
                                EditorGUILayout.LabelField(s);
                            }
                        }
                    ))
                    {
                        using(new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
#if UNITY_EDITOR_OSX
							string buttonName = "Reveal in Finder";
#else
                            string buttonName = "Show in Explorer";
#endif
                            if(GUILayout.Button(buttonName))
                            {
                                EditorUtility.RevealInFinder(exporterNodePath);
                            }
                        }
                    }

                    if(newExportPath != node.Data.ExporterExportPath[currentEditingGroup])
                    {
                        using(new RecordUndoScope("Change Export Path", node, true))
                        {
                            node.Data.ExporterExportPath[currentEditingGroup] = newExportPath;
                        }
                    }
                }
            }
        }



        private void DoInspectorWarpInNode(NodeGUI node)
        {
            EditorGUILayout.HelpBox("Warp-In: Sends the assets to its Warp-Out match.", MessageType.Info);
            UpdateNodeName(node);

            if(GUILayout.Button("Select Out"))
            {
                AssetBundleGraphEditorWindow.SelectNodeById(node.Data.RelatedNodeId);
            }
        }

        private void DoInspectorWarpOutNode(NodeGUI node)
        {
            EditorGUILayout.HelpBox("Warp-Out: Receives the input from its Warp-In match.", MessageType.Info);
            UpdateNodeName(node);

            if(GUILayout.Button("Select In"))
            {
                AssetBundleGraphEditorWindow.SelectNodeById(node.Data.RelatedNodeId);
            }
        }

        private void DoInspectorValidatorGUI(NodeGUI node)
        {
            EditorGUILayout.HelpBox("Validator: Validates assets.", MessageType.Info);
            UpdateNodeName(node);

            GUILayout.Space(10f);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {

                Type incomingType = FindIncomingAssetType(node.Data.InputPoints[0]);

                if(incomingType == null)
                {
                    // if there is no asset input to determine incomingType,
                    // retrieve from assigned Validator.
                    incomingType = ValidatorUtility.GetValidatorTargetType(node.Data.ScriptClassName);

                    if(incomingType == null)
                    {
                        var selected = selectedType == null ? "Select Type" : selectedType.ToString();
                        if(GUILayout.Button(selected, "Popup", GUILayout.MinWidth(220)))
                        {
                            NodeGUI.ShowKeyTypeMenu(selected, x => selectedType = x);
                        }
                        EditorGUILayout.Space();

                        incomingType = selectedType;

                        if(incomingType == null)
                        {
                            EditorGUILayout.HelpBox("Validator needs a single type of incoming assets.", MessageType.Info);
                            return;
                        }
                    }
                }

                var map = ValidatorUtility.GetAttributeClassNameMap(incomingType);
                if(map.Count > 0)
                {
                    using(new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Validator");
                        var guiName = ValidatorUtility.GetValidatorGUIName(node.Data.ScriptClassName);
                        if(GUILayout.Button(guiName, "Popup", GUILayout.MinWidth(150f)))
                        {
                            var builders = map.Keys.ToList();

                            if(builders.Count > 0)
                            {
                                NodeGUI.ShowTypeNamesMenu(guiName, builders, (string selectedGUIName) =>
                                {
                                    using(new RecordUndoScope("Change Validator class", node, true))
                                    {
                                        m_validator = ValidatorUtility.CreateValidator(selectedGUIName, incomingType);
                                        if(m_validator != null)
                                        {
                                            node.Data.ScriptClassName = ValidatorUtility.GUINameToClassName(selectedGUIName, incomingType);
                                            node.Data.InstanceData[currentEditingGroup] = m_validator.Serialize();
                                        }
                                    }
                                }
                                );
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
                else
                {
                    string[] menuNames = AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_VALIDATOR.Split('/');
                    EditorGUILayout.HelpBox(
                        string.Format(
                            "No CustomValidator found for {3} type. \n" +
                            "You need to create at least one Validator script to select script for Validator. " +
                            "To start, select {0}>{1}>{2} menu and create a new script.",
                            menuNames[1], menuNames[2], menuNames[3], incomingType.FullName
                        ), MessageType.Info);
                    return;
                }

                GUILayout.Space(10f);

                if(DrawPlatformSelector(node))
                {
                    // if platform tab is changed, renew Validator Instance for that tab.
                    m_validator = null;
                }
                using(new EditorGUILayout.VerticalScope())
                {
                    var disabledScope = DrawOverrideTargetToggle(node, node.Data.InstanceData.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                    {
                        using(new RecordUndoScope("Override Target Validator", node, true))
                        {
                            if(enabled)
                            {
                                node.Data.InstanceData[currentEditingGroup] = node.Data.InstanceData.DefaultValue;
                            }
                            else
                            {
                                node.Data.InstanceData.Remove(currentEditingGroup);
                            }
                            m_validator = null;
                        }
                    });

                    using(disabledScope)
                    {
                        //reload Validator instance from saved Validator data.
                        if(m_validator == null)
                        {
                            m_validator = ValidatorUtility.CreateValidator(node.Data, currentEditingGroup);
                            if(m_validator != null)
                            {
                                node.Data.ScriptClassName = m_validator.GetType().FullName;
                                if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                {
                                    node.Data.InstanceData[currentEditingGroup] = m_validator.Serialize();
                                }
                            }
                        }

                        if(m_validator != null)
                        {
                            Action onChangedAction = () =>
                            {
                                using(new RecordUndoScope("Change Validator Setting", node, true))
                                {
                                    node.Data.ScriptClassName = m_validator.GetType().FullName;
                                    if(node.Data.InstanceData.ContainsValueOf(currentEditingGroup))
                                    {
                                        node.Data.InstanceData[currentEditingGroup] = m_validator.Serialize();
                                    }
                                }
                            };

                            m_validator.OnInspectorGUI(onChangedAction);
                        }
                    }
                }
            }
        }

        void OnEnable()
        {
            var currentTarget = (NodeGUIInspectorHelper)target;
            var node = currentTarget.node;
            if(node == null) return;

            switch(node.Kind)
            {
            case NodeKind.MODIFIER_GUI:
            case NodeKind.VALIDATOR_GUI:
                {
                    selectedType = null;
                    break;
                }
            }
        }

        void OnLostFocus()
        {
            EditorWindow.GetWindow<AssetBundleGraphEditorWindow>().SaveGraph();
        }



        public override void OnInspectorGUI()
        {
            var currentTarget = (NodeGUIInspectorHelper)target;
            var node = currentTarget.node;
            if(node == null) return;

            switch(node.Kind)
            {
            case NodeKind.LOADER_GUI:
                DoInspectorLoaderGUI(node);
                break;
            case NodeKind.FILTER_GUI:
                DoInspectorFilterGUI(node);
                break;
            case NodeKind.IMPORTSETTING_GUI:
                DoInspectorImportSettingGUI(node);
                break;
            case NodeKind.MODIFIER_GUI:
                DoInspectorModifierGUI(node);
                break;
            case NodeKind.GROUPING_GUI:
                DoInspectorGroupingGUI(node);
                break;
            case NodeKind.PREFABBUILDER_GUI:
                DoInspectorPrefabBuilderGUI(node);
                break;
            case NodeKind.BUNDLECONFIG_GUI:
                DoInspectorBundleConfiguratorGUI(node);
                break;
            case NodeKind.BUNDLEBUILDER_GUI:
                DoInspectorBundleBuilderGUI(node);
                break;
            case NodeKind.EXPORTER_GUI:
                DoInspectorExporterGUI(node);
                break;
            case NodeKind.WARP_IN:
                DoInspectorWarpInNode(node);
                break;
            case NodeKind.WARP_OUT:
                DoInspectorWarpOutNode(node);
                break;
            case NodeKind.VALIDATOR_GUI:
                DoInspectorValidatorGUI(node);
                break;

            default:
                Debug.LogError(node.Name + " is defined as unknown kind of node. value:" + node.Kind);
                break;
            }

            var errors = currentTarget.errors;
            if(errors != null && errors.Any())
            {
                foreach(var error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }

        private Type FindIncomingAssetType(ConnectionPointData inputPoint)
        {
            var assetGroups = AssetBundleGraphEditorWindow.GetIncomingAssetGroups(inputPoint);
            if(assetGroups == null)
            {
                return null;
            }
            return TypeUtility.FindIncomingAssetType(assetGroups.SelectMany(v => v.Value).ToList());
        }

        private void UpdateNodeName(NodeGUI node)
        {
            var newName = EditorGUILayout.TextField("Node Name", node.Name);

            if(NodeGUIUtility.allNodeNames != null)
            {
                var overlapping = NodeGUIUtility.allNodeNames.GroupBy(x => x)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key);
                if(overlapping.Any() && overlapping.Contains(newName))
                {
                    EditorGUILayout.HelpBox("There are node with the same name. You may want to rename to avoid confusion:" + newName, MessageType.Info);
                    AssetBundleGraphEditorWindow.AddNodeException(new NodeException("Node name " + newName + " already exist.", node.Id));
                }
            }

            if(newName != node.Name)
            {
                using(new RecordUndoScope("Change Node Name", node, true))
                {
                    node.Name = newName;
                }
            }
        }

        /*
		 *  Return true if Platform is changed
		 */
        private bool DrawPlatformSelector(NodeGUI node)
        {
            BuildTargetGroup g = currentEditingGroup;
            bool editGroupChanged = false;

            EditorGUI.BeginChangeCheck();
            using(new EditorGUILayout.HorizontalScope())
            {
                var choosenIndex = -1;
                for(var i = 0; i < NodeGUIUtility.platformButtons.Length; i++)
                {
                    var onOffBefore = NodeGUIUtility.platformButtons[i].targetGroup == currentEditingGroup;
                    var onOffAfter = onOffBefore;

                    GUIStyle toolbarbutton = new GUIStyle("toolbarbutton");

                    if(NodeGUIUtility.platformButtons[i].targetGroup == BuildTargetUtility.DefaultTarget)
                    {
                        onOffAfter = GUILayout.Toggle(onOffBefore, NodeGUIUtility.platformButtons[i].ui, toolbarbutton);
                    }
                    else
                    {
                        var width = Mathf.Max(32f, toolbarbutton.CalcSize(NodeGUIUtility.platformButtons[i].ui).x);
                        onOffAfter = GUILayout.Toggle(onOffBefore, NodeGUIUtility.platformButtons[i].ui, toolbarbutton, GUILayout.Width(width));
                    }

                    if(onOffBefore != onOffAfter)
                    {
                        choosenIndex = i;
                        break;
                    }
                }

                if(EditorGUI.EndChangeCheck())
                {
                    g = NodeGUIUtility.platformButtons[choosenIndex].targetGroup;
                }
            }

            if(g != currentEditingGroup)
            {
                currentEditingGroup = g;
                editGroupChanged = true;
                GUI.FocusControl(string.Empty);
            }

            return editGroupChanged;
        }

        private EditorGUI.DisabledGroupScope DrawOverrideTargetToggle(NodeGUI node, bool status, Action<bool> onStatusChange)
        {

            if(currentEditingGroup == BuildTargetUtility.DefaultTarget)
            {
                return new EditorGUI.DisabledGroupScope(false);
            }

            bool newStatus = GUILayout.Toggle(status,
                "Override for " + NodeGUIUtility.GetPlatformButtonFor(currentEditingGroup).ui.tooltip);

            if(newStatus != status && onStatusChange != null)
            {
                onStatusChange(newStatus);
            }
            return new EditorGUI.DisabledGroupScope(!newStatus);
        }
    }
}
