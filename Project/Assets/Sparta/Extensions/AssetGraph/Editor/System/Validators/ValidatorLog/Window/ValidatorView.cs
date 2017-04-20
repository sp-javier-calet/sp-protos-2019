using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AssetBundleGraph
{
    public class ValidatorView : WindowView<ValidatorLogWindow>
    {
        List<ValidatorEntryGUI> entries = new List<ValidatorEntryGUI>();

        const string _fileModifiedTooltip = "The target asset of this validation has been modified after this validation. Those changes could have affected the result.";
        const string _graphModifiedTooltip = "The Graph was modified after this validation. Those changes could have affected the result.";
        const string _validatorRemovedTooltip = "The Validator node that produced this validation is no longer present in the graph.";
        const string _fileRemovedTooltip = "The target asset was removed from the project.";
        const string _remoteInformationTooltip = "This Validation is not local so you cannot re-run it individually, use one of the buttons on the top.";

        const string _graphModifiedKey = "VAL_LOG_GRAPH_MOD_TINT";
        const string _validatorRemovedKey = "VAL_LOG_VAL_REM_TINT";
        const string _fileModifiedKey = "VAL_LOG_FILE_MOD_TINT";
        const string _fileRemovedKey = "VAL_LOG_FILE_REM_TINT";

        public ValidatorLog currentLogInWindow;
        HashSet<BuildTargetGroup> targets = new HashSet<BuildTargetGroup>();

        bool shouldRefresh = false;

        Vector2 scrollPos = Vector2.zero;

        public ValidatorView(ValidatorLogWindow parent) : base(parent)
        {
            LoadValidatorLog(ValidatorController.GetLastValidatorLog());
        }

        public override void OnEnableMethod()
        {
        }

        public override void OnFocusMethod()
        {
            if(currentLogInWindow.isLocal)
            {
                Refresh();
            }
            else
            {
                entries.ForEach(x => x.CheckIsOutdated());
            }
        }

        public void LoadValidatorLog(ValidatorLog log)
        {
            currentLogInWindow = log;
            var saveData = SaveData.LoadFromDisk();
            List<string> objsProcessed = new List<string>();
            Dictionary<string, ValidatorEntryData> entriesToProcess = new Dictionary<string, ValidatorEntryData>(log.entries);
            List<ValidatorEntryGUI> entriesToRemove = new List<ValidatorEntryGUI>();
            targets.Clear();

            for(int i = 0; i < entries.Count; i++)
            {
                var oldEntry = entries[i];
                if(entriesToProcess.ContainsKey(oldEntry.validatorData.Id))
                {
                    var matchingValidator = entriesToProcess[oldEntry.validatorData.Id];
                    InvalidObjectInfo matchingObj = null;

                    matchingObj = matchingValidator.ObjectsWithError.Find(x => x.AssetId == oldEntry.invalidObject.AssetId);

                    if(matchingObj != null)
                    {
                        bool validatorRemoved = saveData.Graph.Nodes.Find(x => x.Id == matchingValidator.ValidatorData.Id) == null;
                        oldEntry.Initialize(matchingValidator.ValidatorData, matchingObj, validatorRemoved, saveData.LastModified);
                        objsProcessed.Add(matchingObj.AssetId);
                    }
                    else
                    {
                        entries.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    entriesToRemove.Add(oldEntry);
                }
            }
            entries.RemoveAll(x => entriesToRemove.Contains(x));

            foreach(var remainingEntry in entriesToProcess.Values)
            {
                foreach(var obj in remainingEntry.ObjectsWithError.FindAll(x => !objsProcessed.Contains(x.AssetId)))
                {
                    bool validatorRemoved = saveData.Graph.Nodes.Find(x => x.Id == remainingEntry.ValidatorData.Id) == null;

                    var validatorEntry = new ValidatorEntryGUI(remainingEntry.ValidatorData, obj, validatorRemoved, saveData.LastModified);
                    entries.Add(validatorEntry);
                }
            }

            foreach(var entry in entries)
            {
                targets.UnionWith(entry.invalidObject.platformInfo.Keys);
            }

            entries.Sort(ValidatorEntryGUI.SortByName);
        }

        void Refresh()
        {
            currentLogInWindow = ValidatorLog.LoadFromDisk();
            LoadValidatorLog(currentLogInWindow);
            shouldRefresh = false;
        }

        void RunAllValidators(BuildTarget target)
        {
            var saveData = SaveData.LoadFromDisk();
            AssetBundleGraphController.Perform(saveData.Graph, target, true, x => Debug.LogError(x), null, null, true);
            Refresh();
            entries.ForEach(x => x.SwitchTargetTab(BuildTargetUtility.TargetToGroup(target)));
        }

        void ValidateSingleAsset(BuildTarget target, string nodeId, string assetPath)
        {
            var nodeExecutor = new IntegratedGUIValidator();

            var saveData = SaveData.LoadFromDisk();
            var validator = saveData.Graph.Nodes.Find(x => x.Id == nodeId);

            nodeExecutor.ValidateSingleAsset(target, validator, Asset.CreateAssetWithImportPath(AssetDatabase.GUIDToAssetPath(assetPath)));
            AssetBundleGraphController.CurrentLog.Save();
            Refresh();
        }

        public override void OnGUIMethod()
        {
            DrawToolBar();
            if(entries.Count == 0)
            {
                EditorGUILayout.Space();
                GUILayout.Label("There are no validation errors!", EditorStyles.centeredGreyMiniLabel);
                return;
            }


            GUIStyle box = new GUIStyle("box");
            box.margin.top = 0;

            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.wordWrap = true;
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;
            GUIStyle msgStyle = new GUIStyle(style);
            msgStyle.alignment = TextAnchor.MiddleLeft;

            using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                EditorGUILayout.Space();
                scrollPos = scroll.scrollPosition;
                for(int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    using(var vScope = new EditorGUILayout.VerticalScope(box))
                    {
                        //EditorGUILayout.LabelField(entry.currentEditingTarget.ToString(), EditorStyles.toolbarButton);
                        GUI.color = Color.white;

                        using(var hScope = new EditorGUILayout.HorizontalScope())
                        {
                            DrawPlatformSelector(entry);

                            var platformButtonsRect = GUILayoutUtility.GetLastRect();

                            var c1Content = entry.validatorData.Name /*+ " - " + entry.validatorData.ScriptClassName*/;
                            var c2Content = entry.invalidObject.AssetName;
                            var c3Content = entry.invalidObject.platformInfo[entry.currentEditingTarget].Message;
                            var bContent = new GUIContent("Re-Validate", NodeGUIUtility.GetPlatformButtonFor(entry.currentEditingTarget).ui.image);
                            bContent.tooltip = entry.currentEditingTarget.ToString();
                            if(!currentLogInWindow.isLocal)
                            {
                                bContent.tooltip = _remoteInformationTooltip;
                            }
                            else if(entry.validatorRemoved)
                            {
                                bContent.tooltip = _validatorRemovedTooltip;
                            }
                            else if(entry.fileRemoved)
                            {
                                bContent.tooltip = _fileRemovedTooltip;
                            }
                            else if(entry.GetCurrentOutdatedInfo().graphChanged)
                            {
                                bContent.tooltip = _graphModifiedTooltip;
                            }
                            else if(entry.GetCurrentOutdatedInfo().fileChanged)
                            {
                                bContent.tooltip = _fileModifiedTooltip;
                            }


                            var rect1 = GUILayoutUtility.GetRect(new GUIContent(c1Content), style, GUILayout.Width(180));
                            var rect2 = GUILayoutUtility.GetRect(new GUIContent(c2Content), style, GUILayout.Width(120));
                            var rect3 = GUILayoutUtility.GetRect(new GUIContent(c3Content), msgStyle, GUILayout.MinWidth(150));
                            var rect4 = GUILayoutUtility.GetRect(bContent, EditorStyles.miniButton, GUILayout.Width(90));

                            var maxHeight = Mathf.Max(platformButtonsRect.height, rect1.height, rect2.height, rect3.height, rect4.height);

                            rect1.height = maxHeight;
                            rect2.height = maxHeight;
                            rect3.height = maxHeight;
                            rect4.y += (maxHeight / 2 - rect4.height / 2);

                            GUI.color = Color.black;
                            GUIStyle separator = new GUIStyle("box");
                            separator.border = new RectOffset(1, 1, 0, 0);
                            var sep0 = new Rect(vScope.rect);
                            sep0.x = platformButtonsRect.xMax + 2; sep0.width = 1;
                            GUI.Box(sep0, "", separator);

                            var sep1 = new Rect(vScope.rect);
                            sep1.x = rect1.xMax; sep1.width = 1;
                            GUI.Box(sep1, "", separator);

                            var sep2 = new Rect(vScope.rect);
                            sep2.x = rect2.xMax; sep2.width = 1;
                            GUI.Box(sep2, "", separator);

                            var sep3 = new Rect(vScope.rect);
                            sep3.x = rect3.xMax; sep3.width = 1;
                            GUI.Box(sep3, "", separator);
                            GUI.color = Color.white;

                            GUI.Label(rect1, c1Content, style);
                            GUI.Label(rect2, c2Content, style);
                            GUI.Label(rect3, c3Content, msgStyle);


                            System.Action btnAction = () => ValidateSingleAsset(BuildTargetUtility.GroupToTarget(entry.currentEditingTarget), entry.validatorData.Id, entry.invalidObject.AssetId);
                            GUI.enabled = currentLogInWindow.isLocal && !entry.GetCurrentOutdatedInfo().graphChanged;

                            if(entry.validatorRemoved || entry.fileRemoved)
                            {
                                GUI.color = Color.red;
                                bContent.text = "Remove";
                                btnAction = () =>
                                {
                                    currentLogInWindow.RemoveSingleEntry(entry.validatorData.Id, entry.invalidObject.AssetId, entry.currentEditingTarget);
                                    if(currentLogInWindow.isLocal)
                                    {
                                        currentLogInWindow.Save();
                                    }
                                    //entries.Remove(entry);
                                    i--;
                                };
                                GUI.enabled = true;
                            }
                            else if(entry.GetCurrentOutdatedInfo().graphChanged || entry.GetCurrentOutdatedInfo().fileChanged)
                            {
                                GUI.color = Color.yellow;
                            }

                            if(GUI.Button(rect4, bContent, EditorStyles.miniButton))
                            {
                                btnAction();
                            }
                        }
                        GUI.color = Color.white;
                        GUI.enabled = true;
                    }
                }
            }

            if(shouldRefresh)
            {
                Refresh();
            }
        }


        void DrawToolBar()
        {
            var styles = new GUIStyle(EditorStyles.toolbar);
            //styles.fixedHeight *= 2;
            using(new EditorGUILayout.HorizontalScope(styles))
            {
                int idx = 0;
                foreach(var targetGroup in currentLogInWindow.executedPlatforms.Keys)
                {
                    var target = BuildTargetUtility.GroupToTarget(targetGroup);
                    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
                    //style.fixedHeight *= 2;

                    var rect = GUILayoutUtility.GetRect(new GUIContent(""), style);

                    var btn = NodeGUIUtility.GetPlatformButtonFor(targetGroup);
                    var content = new GUIContent("Full Validation", btn.ui.image, "Run Graph in validation mode for this Platform");

                    if(currentLogInWindow.executedPlatforms.ContainsKey(targetGroup))
                    {
                        if(!entries.Any(x => x.invalidObject.platformInfo.ContainsKey(targetGroup)))
                        {
                            GUI.color = Color.green;
                        }
                    }

                    if(GUI.Button(rect, content, style))
                    {
                        RunAllValidators(target);
                    }
                    GUI.color = Color.white;
                    idx++;
                }
            }
        }

        /*
        *  Return true if Platform is changed
        */
        private bool DrawPlatformSelector(ValidatorEntryGUI entry)
        {
            var currentEditingTarget = entry.currentEditingTarget;

            BuildTargetGroup g = currentEditingTarget;
            bool editGroupChanged = false;

            EditorGUI.BeginChangeCheck();
            GUIStyle scopeStyle = new GUIStyle();
            scopeStyle.margin = new RectOffset(4, 4, 0, 0);
            scopeStyle.alignment = TextAnchor.MiddleCenter;
            using(new EditorGUILayout.VerticalScope(scopeStyle, GUILayout.Width(32)))
            {
                BuildTargetGroup newGroup = g;
                List<BuildTargetGroup> alreadyPaintedGroups = new List<BuildTargetGroup>();

                foreach(var target in currentLogInWindow.executedPlatforms.Keys)
                {
                    var buildTargetGroup = target;
                    if(alreadyPaintedGroups.Contains(buildTargetGroup))
                    {
                        continue;
                    }

                    alreadyPaintedGroups.Add(buildTargetGroup);
                    var btn = NodeGUIUtility.GetPlatformButtonFor(buildTargetGroup);

                    var onOffBefore = btn.targetGroup == currentEditingTarget;
                    var onOffAfter = onOffBefore;

                    GUIStyle toolbarbutton = new GUIStyle("toolbarbutton");
                    toolbarbutton.fixedHeight = 32;

                    var width = Mathf.Max(32f, toolbarbutton.CalcSize(btn.ui).x);
                    var rect = GUILayoutUtility.GetRect(btn.ui, toolbarbutton, GUILayout.Height(32), GUILayout.Width(width));
                    var content = new GUIContent(btn.ui);

                    if(!entry.invalidObject.platformInfo.Keys.Contains(target))
                    {
                        GUI.color = Color.green;
                        GUI.enabled = false;
                    }
                    else if(entry.validatorRemoved)
                    {
                        content.tooltip = _validatorRemovedTooltip;
                        GUI.color = Color.red * 40;
                    }
                    else if(entry.fileRemoved)
                    {
                        content.tooltip = _fileRemovedTooltip;
                        GUI.color = Color.red * 40;
                    }
                    else if(entry.outdatedInfo[target].graphChanged)
                    {
                        content.tooltip = _graphModifiedTooltip;
                        GUI.color = Color.yellow * 10;
                    }
                    else if(entry.outdatedInfo[target].fileChanged)
                    {
                        content.tooltip = _fileModifiedTooltip;
                        GUI.color = Color.yellow * 10;
                    }

                    onOffAfter = GUI.Toggle(rect, onOffBefore, content, toolbarbutton);

                    if(onOffBefore != onOffAfter)
                    {
                        newGroup = btn.targetGroup;
                        break;
                    }

                    GUI.color = Color.white;
                    GUI.enabled = true;
                }

                if(EditorGUI.EndChangeCheck())
                {
                    g = newGroup;
                }
            }

            if(g != currentEditingTarget)
            {
                entry.currentEditingTarget = g;
                editGroupChanged = true;
                GUI.FocusControl(string.Empty);
            }

            return editGroupChanged;
        }



    }


}
