using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AssetBundleGraph
{
    public class ValidatorView : WindowView<ValidatorLogWindow>
    {
        const string _fileModifiedTooltip = "The target asset of this validation has been modified after this validation. Those changes could have affected the result.";
        const string _graphModifiedTooltip = "The Graph was modified after this validation. Those changes could have affected the result.";
        const string _validatorRemovedTooltip = "The Validator node that produced this validation is no longer present in the graph.";
        const string _fileRemovedTooltip = "The target asset was removed from the project.";
        const string _remoteInformationTooltip = "This Validation is not local so you cannot re-run it individually, use one of the buttons on the top.";

        const string _graphModifiedKey = "VAL_LOG_GRAPH_MOD_TINT";
        const string _validatorRemovedKey = "VAL_LOG_VAL_REM_TINT";
        const string _fileModifiedKey = "VAL_LOG_FILE_MOD_TINT";
        const string _fileRemovedKey = "VAL_LOG_FILE_REM_TINT";

        public ValidatorLog CurrentLogInWindow;

        List<ValidatorEntryGUI> _entries = new List<ValidatorEntryGUI>();
        bool _shouldRefresh = false;
        Vector2 _scrollPos = Vector2.zero;

        public ValidatorView(ValidatorLogWindow parent) : base(parent)
        {
            LoadValidatorLog(ValidatorController.GetLastValidatorLog());
        }

        public override void OnEnableMethod()
        {
        }

        public override void OnFocusMethod()
        {
            if(CurrentLogInWindow.IsLocal)
            {
                Refresh();
            }
            else
            {
                _entries.ForEach(x => x.CheckIsOutdated());
            }
        }

        public void LoadValidatorLog(ValidatorLog log)
        {
            CurrentLogInWindow = log;
            var saveData = SaveData.LoadFromDisk();
            List<string> objsProcessed = new List<string>();
            Dictionary<string, ValidatorEntryData> entriesToProcess = new Dictionary<string, ValidatorEntryData>(log.entries);
            List<ValidatorEntryGUI> entriesToRemove = new List<ValidatorEntryGUI>();

            AssetDatabase.Refresh();

            for(int i = 0; i < _entries.Count; i++)
            {
                var oldEntry = _entries[i];
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
                        _entries.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    entriesToRemove.Add(oldEntry);
                }
            }
            _entries.RemoveAll(x => entriesToRemove.Contains(x));

            foreach(var remainingEntry in entriesToProcess.Values)
            {
                foreach(var obj in remainingEntry.ObjectsWithError.FindAll(x => !objsProcessed.Contains(x.AssetId)))
                {
                    bool validatorRemoved = saveData.Graph.Nodes.Find(x => x.Id == remainingEntry.ValidatorData.Id) == null;

                    var validatorEntry = new ValidatorEntryGUI(remainingEntry.ValidatorData, obj, validatorRemoved, saveData.LastModified);
                    _entries.Add(validatorEntry);
                }
            }

            _entries.Sort(ValidatorEntryGUI.SortByName);
        }

        void Refresh()
        {
            CurrentLogInWindow = ValidatorLog.LoadFromDisk();
            LoadValidatorLog(CurrentLogInWindow);
            _shouldRefresh = false;
        }

        void RunAllValidators(BuildTarget target)
        {
            var saveData = SaveData.LoadFromDisk();
            AssetBundleGraphController.Perform(saveData.Graph, target, true, x => Debug.LogError(x), null, null, true);
            Refresh();
            _entries.ForEach(x => x.SwitchTargetTab(BuildTargetUtility.TargetToGroup(target)));
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
            if(_entries.Count == 0)
            {
                EditorGUILayout.Space();
                GUILayout.Label("There are no validation errors!", EditorStyles.centeredGreyMiniLabel);
                return;
            }


            GUIStyle box = new GUIStyle("box");
            box.margin.top = 0;


            GUIStyle style = new GUIStyle(EditorStyles.label);

            style.wordWrap = true;
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;

            GUIStyle link = new GUIStyle(style);
            link.normal.textColor = new Color(0.29f, 0.49f, 1f);
            link.active.textColor = new Color(0.5f, 0.7f, 1f);
            GUIStyle brokenlink = new GUIStyle(link);
            brokenlink.normal.textColor = new Color(1f, 0.23f, 0.05f);
            brokenlink.active.textColor = new Color(1f, 0.53f, 0.3f);

            GUIStyle msgStyle = new GUIStyle(style);
            msgStyle.alignment = TextAnchor.MiddleLeft;

            using(var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                EditorGUILayout.Space();
                _scrollPos = scroll.scrollPosition;
                for(int i = 0; i < _entries.Count; i++)
                {
                    var entry = _entries[i];
                    using(var vScope = new EditorGUILayout.VerticalScope(box))
                    {
                        GUI.color = Color.white;

                        using(var hScope = new EditorGUILayout.HorizontalScope())
                        {
                            DrawPlatformSelector(entry);

                            var platformButtonsRect = GUILayoutUtility.GetLastRect();

                            var c1Content = entry.validatorData.Name;
                            var c2Content = entry.invalidObject.AssetName;
                            var c3Content = entry.invalidObject.platformInfo[entry.currentEditingTarget].Message;
                            var bContent = new GUIContent("Re-Validate", NodeGUIUtility.GetPlatformButtonFor(entry.currentEditingTarget).ui.image);
                            bContent.tooltip = entry.currentEditingTarget.ToString();
                            if(!CurrentLogInWindow.IsLocal)
                            {
                                bContent.tooltip = _remoteInformationTooltip;
                            }
                            else if(entry.ValidatorRemoved)
                            {
                                bContent.tooltip = _validatorRemovedTooltip;
                            }
                            else if(entry.FileRemoved)
                            {
                                bContent.tooltip = _fileRemovedTooltip;
                            }
                            else if(entry.GetCurrentOutdatedInfo().GraphChanged)
                            {
                                bContent.tooltip = _graphModifiedTooltip;
                            }
                            else if(entry.GetCurrentOutdatedInfo().FileChanged)
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

                            if(GUI.Button(rect1, c1Content, entry.ValidatorRemoved ? brokenlink : link))
                            {
                                if(entry.ValidatorRemoved)
                                {
                                    Debug.LogError("This validator no longer exists in AssetGraph");
                                }
                                else
                                {
                                    AssetBundleGraphEditorWindow.SelectNodeById(entry.validatorData.Id);
                                }
                            }
                            if(GUI.Button(rect2, c2Content, entry.FileRemoved ? brokenlink : link))
                            {
                                if(entry.FileRemoved)
                                {
                                    Debug.LogError("This object no longer exists in the project or was moved");
                                }
                                else
                                {
                                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(entry.invalidObject.AssetId));
                                }
                            }

                            GUI.Label(rect3, c3Content, msgStyle);

                            EditorGUIUtility.AddCursorRect(rect2, MouseCursor.Link);
                            EditorGUIUtility.AddCursorRect(rect1, MouseCursor.Link);

                            System.Action btnAction = () => ValidateSingleAsset(BuildTargetUtility.GroupToTarget(entry.currentEditingTarget), entry.validatorData.Id, entry.invalidObject.AssetId);
                            GUI.enabled = CurrentLogInWindow.IsLocal && !entry.GetCurrentOutdatedInfo().GraphChanged;

                            if(entry.ValidatorRemoved || entry.FileRemoved)
                            {
                                GUI.color = new Color(1f, 0.23f, 0.05f);
                                bContent.text = "Remove";
                                btnAction = () =>
                                {
                                    CurrentLogInWindow.RemoveSingleEntry(entry.validatorData.Id, entry.invalidObject.AssetId, entry.currentEditingTarget);
                                    if(CurrentLogInWindow.IsLocal)
                                    {
                                        CurrentLogInWindow.Save();
                                    }
                                    i--;
                                };
                                GUI.enabled = true;
                            }
                            else if(entry.GetCurrentOutdatedInfo().GraphChanged || entry.GetCurrentOutdatedInfo().FileChanged)
                            {
                                GUI.color = new Color(1f, 0.92f, 0.02f);
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

            if(_shouldRefresh)
            {
                Refresh();
            }
        }


        void DrawToolBar()
        {
            if(CurrentLogInWindow.executedPlatforms.Count == 0)
            {
                return;
            }

            var styles = new GUIStyle(EditorStyles.toolbar);

            using(new EditorGUILayout.HorizontalScope(styles))
            {
                int idx = 0;
                foreach(var targetGroup in CurrentLogInWindow.executedPlatforms.Keys)
                {
                    var target = BuildTargetUtility.GroupToTarget(targetGroup);
                    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);

                    var rect = GUILayoutUtility.GetRect(new GUIContent(""), style);

                    var btn = NodeGUIUtility.GetPlatformButtonFor(targetGroup);
                    var content = new GUIContent("Full Validation", btn.ui.image, "Run Graph in validation mode for this Platform");

                    if(CurrentLogInWindow.executedPlatforms.ContainsKey(targetGroup))
                    {
                        if(!_entries.Any(x => x.invalidObject.platformInfo.ContainsKey(targetGroup)))
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

                foreach(var target in CurrentLogInWindow.executedPlatforms.Keys)
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
                    else if(entry.ValidatorRemoved)
                    {
                        content.tooltip = _validatorRemovedTooltip;
                        GUI.color = new Color(1f, 0.23f, 0.05f);
                    }
                    else if(entry.FileRemoved)
                    {
                        content.tooltip = _fileRemovedTooltip;
                        GUI.color = new Color(1f, 0.23f, 0.05f);
                    }
                    else if(entry.outdatedInfo[target].GraphChanged)
                    {
                        content.tooltip = _graphModifiedTooltip;
                        GUI.color = new Color(1f, 0.92f, 0.02f);
                    }
                    else if(entry.outdatedInfo[target].FileChanged)
                    {
                        content.tooltip = _fileModifiedTooltip;
                        GUI.color = new Color(1f, 0.92f, 0.02f);
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
