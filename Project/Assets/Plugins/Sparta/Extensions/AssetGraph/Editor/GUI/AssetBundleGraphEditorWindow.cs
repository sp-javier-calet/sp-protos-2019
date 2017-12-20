using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AssetBundleGraph
{
    public class AssetBundleGraphEditorWindow : EditorWindow
    {

        [Serializable]
        public struct KeyObject
        {
            public string key;

            public KeyObject(string val)
            {
                key = val;
            }
        }

        [Serializable]
        public struct ActiveObject
        {
            [SerializeField]
            public SerializableVector2Dictionary idPosDict;

            public ActiveObject(Dictionary<string, Vector2> idPosDict)
            {
                this.idPosDict = new SerializableVector2Dictionary(idPosDict);
            }
        }

        [Serializable]
        public struct CopyField
        {
            [SerializeField]
            public List<string> datas;
            [SerializeField]
            public CopyType type;

            public CopyField(List<string> datas, CopyType type)
            {
                this.datas = datas;
                this.type = type;
            }
        }

        // hold selection start data.
        public struct AssetBundleGraphSelection
        {
            public readonly float x;
            public readonly float y;

            public AssetBundleGraphSelection(Vector2 position)
            {
                this.x = position.x;
                this.y = position.y;
            }
        }

        // hold scale start data.
        public struct ScalePoint
        {
            public readonly float x;
            public readonly float y;
            public readonly float startScale;
            public readonly int scaledDistance;

            public ScalePoint(Vector2 point, float scaleFactor, int scaledDistance)
            {
                this.x = point.x;
                this.y = point.y;
                this.startScale = scaleFactor;
                this.scaledDistance = scaledDistance;
            }
        }

        public enum ModifyMode : int
        {
            NONE,
            CONNECTING,
            SELECTING,
            SCALING,
            SCROLLING
        }

        public enum CopyType : int
        {
            COPYTYPE_COPY,
            COPYTYPE_CUT
        }

        public enum ScriptType : int
        {
            SCRIPT_VALIDATOR,
            SCRIPT_MODIFIER,
            SCRIPT_PREFABBUILDER,
            SCRIPT_POSTPROCESS
        }


        [SerializeField]
        private GraphGUI graphGUI;
        [SerializeField]
        private ActiveObject activeObject = new ActiveObject(new Dictionary<string, Vector2>());

        [SerializeField]
        private BuildTarget selectedTarget;

        private bool showErrors;
        private NodeEvent currentEventSource;
        private Texture2D _selectionTex;
        private GUIContent _reloadButtonTexture;
        private ModifyMode modifyMode;
        private DateTime lastLoaded = DateTime.MinValue;
        private Vector2 spacerRectRightBottom;
        private Vector2 scrollPos = new Vector2(0, 0);
        private Vector2 errorScrollPos = new Vector2(0, 0);
        private Rect graphRegion = new Rect();
        private CopyField copyField = new CopyField();
        private AssetBundleGraphSelection selection;
        private ScalePoint scalePoint;
        private GraphBackground background = new GraphBackground();
        private bool _compiled = false;
        private double lastClickedTime = 0;
        private double doubleClickTime = 0.3f;

        private Vector2 deltaScrollPos = new Vector2(0, 0);

        private static Dictionary<ConnectionData, Dictionary<string, List<Asset>>> s_assetStreamMap =
            new Dictionary<ConnectionData, Dictionary<string, List<Asset>>>();
        private static List<NodeException> s_nodeExceptionPool = new List<NodeException>();

        private Texture2D selectionTex
        {
            get
            {
                if(_selectionTex == null)
                {
                    _selectionTex = LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_SELECTION);
                }
                return _selectionTex;
            }
        }

        private GUIContent reloadButtonTexture
        {
            get
            {
                if(_reloadButtonTexture == null)
                {
                    _reloadButtonTexture = EditorGUIUtility.IconContent("RotateTool");
                }
                return _reloadButtonTexture;
            }
        }

        public static void GenerateScript(ScriptType scriptType)
        {
            var destinationBasePath = AssetBundleGraphSettings.USERSPACE_PATH;
            var destinationPath = string.Empty;

            var sourceFileName = string.Empty;

            switch(scriptType)
            {
            case ScriptType.SCRIPT_VALIDATOR:
                {
                    sourceFileName = FileUtility.PathCombine(AssetGraphRelativePaths.SCRIPT_TEMPLATE_PATH, "MyValidator.cs.template");
                    destinationPath = FileUtility.PathCombine(destinationBasePath, "MyValidator.cs");
                    break;
                }
            case ScriptType.SCRIPT_MODIFIER:
                {
                    sourceFileName = FileUtility.PathCombine(AssetGraphRelativePaths.SCRIPT_TEMPLATE_PATH, "MyModifier.cs.template");
                    destinationPath = FileUtility.PathCombine(destinationBasePath, "MyModifier.cs");
                    break;
                }
            case ScriptType.SCRIPT_PREFABBUILDER:
                {
                    sourceFileName = FileUtility.PathCombine(AssetGraphRelativePaths.SCRIPT_TEMPLATE_PATH, "MyPrefabBuilder.cs.template");
                    destinationPath = FileUtility.PathCombine(destinationBasePath, "MyPrefabBuilder.cs");
                    break;
                }
            case ScriptType.SCRIPT_POSTPROCESS:
                {
                    sourceFileName = FileUtility.PathCombine(AssetGraphRelativePaths.SCRIPT_TEMPLATE_PATH, "MyPostprocess.cs.template");
                    destinationPath = FileUtility.PathCombine(destinationBasePath, "MyPostprocess.cs");
                    break;
                }
            default:
                {
                    Debug.LogError("Unknown script type found:" + scriptType);
                    break;
                }
            }

            if(string.IsNullOrEmpty(sourceFileName))
            {
                return;
            }

            FileUtility.CopyFileFromGlobalToLocal(sourceFileName, destinationPath);

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destinationPath);

        }

        /*
			menu items
		*/
        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_MODIFIER, false, 7)]
        public static void GenerateModifier()
        {
            GenerateScript(ScriptType.SCRIPT_MODIFIER);
        }
        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_VALIDATOR, false, 7)]
        public static void GenerateValidator()
        {
            GenerateScript(ScriptType.SCRIPT_VALIDATOR);
        }
        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_PREFABBUILDER, false, 10)]
        public static void GeneratePrefabBuilder()
        {
            GenerateScript(ScriptType.SCRIPT_PREFABBUILDER);
        }
        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_GENERATE_POSTPROCESS, false, 10)]
        public static void GeneratePostprocess()
        {
            GenerateScript(ScriptType.SCRIPT_POSTPROCESS);
        }

        [MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_OPEN, false, 1)]
        public static void Open()
        {
            NodeGUI.scaleFactor = 1f;
            var window = GetWindow<AssetBundleGraphEditorWindow>();
            window.InitializeGraph();
        }

        //[MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_BUILD, true, 1 + 11)]
        //public static bool BuildFromMenuValidator () {
        //	// Calling GetWindow<>() will force open window
        //	// That's not what we want to do in validator function,
        //	// so just reference s_nodeExceptionPool directly
        //	return (s_nodeExceptionPool != null && s_nodeExceptionPool.Count == 0);
        //}

        //[MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_BUILD, false, 1 + 11)]
        //public static void BuildFromMenu () {
        //	var window = GetWindow<AssetBundleGraphEditorWindow>();
        //	window.Run(window.ActiveBuildTarget);
        //}

        //[MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_DELETE_CACHE)] public static void DeleteCache () {
        //	FileUtility.RemakeDirectory(AssetBundleGraphSettings.APPLICATIONDATAPATH_CACHE_PATH);

        //	AssetDatabase.Refresh();
        //}

        //[MenuItem(AssetBundleGraphSettings.GUI_TEXT_MENU_DELETE_IMPORTSETTING_SETTINGS)] public static void DeleteImportSettingSample () {
        //	FileUtility.RemakeDirectory(AssetBundleGraphSettings.IMPORTER_SETTINGS_PLACE);

        //	AssetDatabase.Refresh();
        //}

        public BuildTarget ActiveBuildTarget
        {
            get
            {
                return selectedTarget;
            }
        }

        public void OnFocus()
        {
            // update handlers. these static handlers are erase when window is full-screened and badk to normal window.
            modifyMode = ModifyMode.NONE;
            NodeGUIUtility.NodeEventHandler = HandleNodeEvent;
            ConnectionGUIUtility.ConnectionEventHandler = HandleConnectionEvent;
        }

        public void OnLostFocus()
        {
            modifyMode = ModifyMode.NONE;
        }

        public void SelectNode(string nodeId)
        {
            var selectObject = graphGUI.Nodes.Find(node => node.Id == nodeId);
            // set deactive for all nodes.
            foreach(var node in graphGUI.Nodes)
            {
                node.SetInactive();
            }
            if(selectObject != null)
            {
                selectObject.SetActive();
                Selection.activeObject = selectObject.NodeInspectorHelper;
            }
        }

        public static void UpdateConnectionInspector(ConnectionGUI con)
        {
            var keyEnum = s_assetStreamMap.Keys.Where(c => c.Id == con.Id);
            if(keyEnum.Any())
            {
                var assets = s_assetStreamMap[keyEnum.First()];
                con.ConnectionInspectorHelper.UpdateInspector(con, assets);
            }
        }


        private void Init()
        {
            this.titleContent = new GUIContent("AssetGraph");
            this.selectedTarget = EditorUserBuildSettings.activeBuildTarget;

            Undo.undoRedoPerformed += () =>
            {
                SaveGraphThatRequiresReload();
                Repaint();
            };

            modifyMode = ModifyMode.NONE;
            NodeGUIUtility.NodeEventHandler = HandleNodeEvent;
            ConnectionGUIUtility.ConnectionEventHandler = HandleConnectionEvent;

            InitializeGraph();

            if(graphGUI.Nodes.Any())
            {
                UpdateSpacerRect();
            }
        }

        public static void AddNodeException(NodeException nodeEx)
        {
            s_nodeExceptionPool.Add(nodeEx);
        }

        private static void ResetNodeExceptionPool()
        {
            s_nodeExceptionPool.Clear();
        }

        private bool isAnyIssueFound
        {
            get
            {
                return s_nodeExceptionPool.Count > 0;
            }
        }

        private void ShowErrorOnNodes()
        {
            foreach(var node in graphGUI.Nodes)
            {
                node.ResetErrorStatus();
                var errorsForeachNode = s_nodeExceptionPool.Where(e => e.Id == node.Id).Select(e => e.reason).ToList();
                if(errorsForeachNode.Any())
                {
                    node.AppendErrorSources(errorsForeachNode);
                }
            }
        }

        public static Texture2D LoadTextureFromFile(string path)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }

        private ActiveObject RenewActiveObject(List<string> ids)
        {
            var idPosDict = new Dictionary<string, Vector2>();
            foreach(var node in graphGUI.Nodes)
            {
                if(ids.Contains(node.Id)) idPosDict[node.Id] = node.GetPos();
            }
            foreach(var connection in graphGUI.Connections)
            {
                if(ids.Contains(connection.Id)) idPosDict[connection.Id] = Vector2.zero;
            }
            return new ActiveObject(idPosDict);
        }

        /**
			node graph initializer.
			setup nodes, points and connections from saved data.
		*/
        public void InitializeGraph()
        {
            SaveData saveData = SaveData.LoadFromDisk();
            var graph = saveData.Graph;
            /*
				do nothing if json does not modified after first load.
			*/
            if(saveData.LastModified == lastLoaded)
            {
                return;
            }

            lastLoaded = saveData.LastModified;
            minSize = new Vector2(600f, 300f);

            wantsMouseMove = true;
            modifyMode = ModifyMode.NONE;


            /*
				load graph data from deserialized data.
			*/

            graphGUI = new GraphGUI(graph);
            _compiled = false;
        }

        /**
		 * Get WindowId does not collide with other nodeGUIs
		 */
        private static int GetSafeWindowId(List<NodeGUI> nodeGUIs)
        {
            int id = -1;

            foreach(var nodeGui in nodeGUIs)
            {
                if(nodeGui.WindowId > id)
                {
                    id = nodeGui.WindowId;
                }
            }
            return id + 1;
        }

        public void SaveGraph()
        {
            SaveData newSaveData = new SaveData(graphGUI.Nodes, graphGUI.Connections);
            newSaveData.Save();
        }

        private void SaveGraphThatRequiresReload(bool silent = false)
        {
            _compiled = false;
            PreProcessor.MarkForReload();
            SaveGraph();
        }


        private void Setup(BuildTarget target)
        {

            EditorUtility.ClearProgressBar();

            try
            {
                ResetNodeExceptionPool();

                foreach(var node in graphGUI.Nodes)
                {
                    node.HideProgress();
                }

                // reload data from file.
                var saveData = SaveData.LoadFromDisk();
                Graph graph = saveData.Graph;

                // update static all node names.
                NodeGUIUtility.allNodeNames = new List<string>(graphGUI.Nodes.Select(node => node.Name).ToList());

                Action<NodeException> errorHandler = (NodeException e) =>
                {
                    AddNodeException(e);
                };

                s_assetStreamMap = AssetBundleGraphController.Perform(graph, target, false, errorHandler, null);

                RefreshInspector(s_assetStreamMap);
                ShowErrorOnNodes();

                AssetBundleGraphController.Postprocess(graph, s_assetStreamMap, false);

                _compiled = true;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /**
		 * Execute the build.
		 */
        private void Run(BuildTarget target, List<string> nodeIds = null)
        {

            try
            {
                ResetNodeExceptionPool();

                if(!SaveData.IsSaveDataAvailableAtDisk())
                {
                    SaveData.RecreateDataOnDisk();
                    Debug.Log("AssetBundleGraph save data not found. Creating from scratch...");
                    return;
                }

                // load data from file.
                var saveData = SaveData.LoadFromDisk();
                var graph = saveData.Graph;

                if(nodeIds != null)
                {
                    var rootNodes = graph.Nodes.FindAll(x => nodeIds.Contains(x.Id));
                    graph = graph.GetSubGraph(rootNodes.ToArray());
                }

                var subgraphGUI = new GraphGUI(graph);


                var currentCount = 0.00f;
                var totalCount = subgraphGUI.Nodes.Count * 1f;

                Action<NodeData, float> updateHandler = (node, progress) =>
                {
                    var progressPercentage = ((currentCount / totalCount) * 100).ToString();
                    if(progressPercentage.Contains(".")) progressPercentage = progressPercentage.Split('.')[0];

                    if(0 < progress)
                    {
                        currentCount = currentCount + 1f;
                    }

                    EditorUtility.DisplayProgressBar("AssetBundleGraph Processing... ", "Processing " + node.Name + ": " + progressPercentage + "%", currentCount / totalCount);
                };

                Action<NodeException> errorHandler = (NodeException e) =>
                {
                    AssetBundleGraphEditorWindow.AddNodeException(e);
                };

                // perform setup. Fails if any exception raises.
                s_assetStreamMap = AssetBundleGraphController.Perform(graph, target, false, errorHandler, null);

                // if there is not error reported, then run
                if(s_nodeExceptionPool.Count == 0)
                {
                    // run datas.
                    s_assetStreamMap = AssetBundleGraphController.Perform(graph, target, true, errorHandler, updateHandler, null, false, nodeIds != null);
                }
                RefreshInspector(s_assetStreamMap);
                AssetDatabase.Refresh();
                ShowErrorOnNodes();
                AssetBundleGraphController.Postprocess(graph, s_assetStreamMap, true);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void RefreshInspector(Dictionary<ConnectionData, Dictionary<string, List<Asset>>> currentResult)
        {
            if(Selection.activeObject == null)
            {
                return;
            }

            switch(Selection.activeObject.GetType().ToString())
            {
            case "AssetBundleGraph.ConnectionGUIInspectorHelper":
                {
                    var con = ((ConnectionGUIInspectorHelper)Selection.activeObject).connectionGUI;

                    // null when multiple connection deleted.
                    if(con == null || string.IsNullOrEmpty(con.Id))
                    {
                        return;
                    }

                    ConnectionData c = currentResult.Keys.ToList().Find(v => v.Id == con.Id);

                    if(c != null)
                    {
                        ((ConnectionGUIInspectorHelper)Selection.activeObject).UpdateAssetGroups(currentResult[c]);
                    }
                    break;
                }
            default:
                {
                    // do nothing.
                    break;
                }
            }
        }

        public static Dictionary<string, List<Asset>> GetIncomingAssetGroups(ConnectionPointData inputPoint)
        {
            UnityEngine.Assertions.Assert.IsNotNull(inputPoint);
            UnityEngine.Assertions.Assert.IsTrue(inputPoint.IsInput);

            if(s_assetStreamMap == null)
            {
                return null;
            }

            var keyEnum = s_assetStreamMap.Keys.Where(c => c.ToNodeConnectionPointId == inputPoint.Id);
            if(keyEnum.Any())
            {
                return s_assetStreamMap[keyEnum.First()];
            }

            return null;
        }

        private void DrawGUIToolBar()
        {
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbarButton);

                tbLabel.alignment = TextAnchor.MiddleCenter;

                GUIStyle tbLabelTarget = new GUIStyle(tbLabel);
                tbLabelTarget.fontStyle = FontStyle.Bold;

                if(GUILayout.Button(new GUIContent("Refresh", reloadButtonTexture.image, "Refresh and reload"), EditorStyles.toolbarButton, GUILayout.Width(80), GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT)))
                {
                    Setup(ActiveBuildTarget);
                }
                showErrors = GUILayout.Toggle(showErrors, "Show Error", EditorStyles.toolbarButton, GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT));

                GUILayout.Label("Zoom:", tbLabel);

                var rect = GUILayoutUtility.GetRect(100, 50);
                rect.x += 10;

                NodeGUI.scaleFactor = GUI.HorizontalSlider(rect, NodeGUI.scaleFactor, 0.2f, 1f);
                GUILayout.FlexibleSpace();

                if(isAnyIssueFound)
                {
                    GUIStyle errorStyle = new GUIStyle("ErrorLabel");
                    errorStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("All errors needs to be fixed before building", errorStyle);
                    GUILayout.FlexibleSpace();
                }


                GUILayout.Label("Platform:", tbLabel, GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT));


                var supportedTargets = NodeGUIUtility.SupportedBuildTargets;
                int currentIndex = Mathf.Max(0, supportedTargets.FindIndex(t => t == selectedTarget));

                int newIndex = EditorGUILayout.Popup(currentIndex, NodeGUIUtility.supportedBuildTargetNames,
                    EditorStyles.toolbarButton, GUILayout.Width(150), GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT));

                if(newIndex != currentIndex)
                {
                    selectedTarget = supportedTargets[newIndex];
                }

                using(new EditorGUI.DisabledGroupScope(_compiled))
                {
                    if(GUILayout.Button("Compile", EditorStyles.toolbarButton, GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT)))
                    {
                        SaveGraph();
                        Setup(ActiveBuildTarget);
                    }
                }

                using(new EditorGUI.DisabledGroupScope(!(Selection.objects.Length > 0 && Selection.objects.All(x => x is NodeGUIInspectorHelper && ((NodeGUIInspectorHelper)x).node.Kind == NodeKind.LOADER_GUI)) || !_compiled))
                {
                    if(GUILayout.Button("Run Selected", EditorStyles.toolbarButton, GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT)))
                    {
                        SaveGraph();
                        var selectedLoaderIds = Array.ConvertAll(Selection.objects.Cast<NodeGUIInspectorHelper>().ToArray(), x => x.node.Id);
                        Run(ActiveBuildTarget, selectedLoaderIds.ToList());
                    }
                }
                using(new EditorGUI.DisabledGroupScope(isAnyIssueFound || !_compiled))
                {
                    if(GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.Height(AssetGraphRelativePaths.TOOLBAR_HEIGHT)))
                    {
                        Run(ActiveBuildTarget);
                    }
                }
            }
        }


        private void DrawGUINodeErrors()
        {

            errorScrollPos = EditorGUILayout.BeginScrollView(errorScrollPos, GUI.skin.box, GUILayout.Width(200));
            {
                using(new EditorGUILayout.VerticalScope())
                {
                    foreach(NodeException e in s_nodeExceptionPool)
                    {
                        EditorGUILayout.HelpBox(e.reason, MessageType.Error);
                        if(GUILayout.Button("Go to Node"))
                        {
                            SelectNodeById(e.Id);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void DrawGUINodeGraph()
        {

            background.Draw(graphRegion, scrollPos);

            using(var scrollScope = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollScope.scrollPosition;
                // draw node window x N.
                {
                    BeginWindows();

                    graphGUI.Nodes.ForEach(node => node.DrawNode());

                    EndWindows();
                }

                // draw connection input point marks.
                foreach(var node in graphGUI.Nodes)
                {
                    if(node.Kind != NodeKind.WARP_OUT)
                    {
                        node.DrawConnectionInputPointMark(currentEventSource, modifyMode == ModifyMode.CONNECTING);
                    }
                }

                // draw connections.
                foreach(var con in graphGUI.Connections)
                {
                    var keyEnum = s_assetStreamMap.Keys.Where(c => c.Id == con.Id);
                    if(keyEnum.Any())
                    {
                        var assets = s_assetStreamMap[keyEnum.First()];
                        con.DrawConnection(graphGUI.Nodes, assets);
                    }
                    else
                    {
                        con.DrawConnection(graphGUI.Nodes, new Dictionary<string, List<Asset>>());
                    }
                }

                // draw connection output point marks.
                foreach(var node in graphGUI.Nodes)
                {
                    if(node.Kind != NodeKind.WARP_IN)
                    {
                        node.DrawConnectionOutputPointMark(currentEventSource, modifyMode == ModifyMode.CONNECTING, Event.current);
                    }
                }

                // draw connecting line if modifing connection.
                switch(modifyMode)
                {
                case ModifyMode.CONNECTING:
                    {
                        // from start node to mouse.
                        DrawStraightLineFromCurrentEventSourcePointTo(Event.current.mousePosition, currentEventSource);
                        break;
                    }
                case ModifyMode.SELECTING:
                    {
                        GUI.DrawTexture(new Rect(selection.x, selection.y, Event.current.mousePosition.x - selection.x, Event.current.mousePosition.y - selection.y), selectionTex);
                        break;
                    }
                }

                // handle Graph GUI events
                HandleGraphGUIEvents();


                // set rect for scroll.
                if(graphGUI.Nodes.Any())
                {
                    GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Width(spacerRectRightBottom.x), GUILayout.Height(spacerRectRightBottom.y));
                }
            }
            if(Event.current.type == EventType.Repaint)
            {
                scrollPos += deltaScrollPos;
                deltaScrollPos = Vector2.zero;

                var newRgn = GUILayoutUtility.GetLastRect();
                if(newRgn != graphRegion)
                {
                    graphRegion = newRgn;
                    Repaint();
                }
            }
        }

        private void HandleGraphGUIEvents()
        {

            //mouse drag event handling.
            switch(Event.current.type)
            {

            // draw line while dragging.
            case EventType.MouseDrag:
                {
                    switch(modifyMode)
                    {
                    case ModifyMode.NONE:
                        {
                            switch(Event.current.button)
                            {
                            case 0:
                                {// left click
                                    if(Event.current.command)
                                    {
                                        scalePoint = new ScalePoint(Event.current.mousePosition, NodeGUI.scaleFactor, 0);
                                        modifyMode = ModifyMode.SCALING;
                                        break;
                                    }

                                    selection = new AssetBundleGraphSelection(Event.current.mousePosition);
                                    modifyMode = ModifyMode.SELECTING;
                                    break;
                                }
                            case 2:
                                {// middle click.
                                    modifyMode = ModifyMode.SCROLLING;
                                    break;
                                }
                            }
                            break;
                        }
                    case ModifyMode.SELECTING:
                        {
                            // do nothing.
                            break;
                        }
                    case ModifyMode.SCALING:
                        {
                            var baseDistance = (int)Vector2.Distance(Event.current.mousePosition, new Vector2(scalePoint.x, scalePoint.y));
                            var distance = baseDistance / NodeGUI.SCALE_WIDTH;
                            var direction = (0 < Event.current.mousePosition.y - scalePoint.y);

                            if(!direction) distance = -distance;

                            // var before = NodeGUI.scaleFactor;
                            NodeGUI.scaleFactor = scalePoint.startScale + (distance * NodeGUI.SCALE_RATIO);

                            if(NodeGUI.scaleFactor < NodeGUI.SCALE_MIN) NodeGUI.scaleFactor = NodeGUI.SCALE_MIN;
                            if(NodeGUI.SCALE_MAX < NodeGUI.scaleFactor) NodeGUI.scaleFactor = NodeGUI.SCALE_MAX;
                            break;
                        }
                    case ModifyMode.SCROLLING:
                        {
                            deltaScrollPos += -Event.current.delta;
                            break;
                        }
                    }

                    HandleUtility.Repaint();
                    Event.current.Use();
                    break;
                }
            }

            // mouse up event handling.
            // use rawType for detect for detectiong mouse-up which raises outside of window.
            switch(Event.current.rawType)
            {
            case EventType.MouseUp:
                {
                    switch(modifyMode)
                    {
                    /*
                                select contained nodes & connections.
                            */
                    case ModifyMode.SELECTING:
                        {
                            var x = 0f;
                            var y = 0f;
                            var width = 0f;
                            var height = 0f;

                            if(Event.current.mousePosition.x < selection.x)
                            {
                                x = Event.current.mousePosition.x;
                                width = selection.x - Event.current.mousePosition.x;
                            }
                            if(selection.x < Event.current.mousePosition.x)
                            {
                                x = selection.x;
                                width = Event.current.mousePosition.x - selection.x;
                            }

                            if(Event.current.mousePosition.y < selection.y)
                            {
                                y = Event.current.mousePosition.y;
                                height = selection.y - Event.current.mousePosition.y;
                            }
                            if(selection.y < Event.current.mousePosition.y)
                            {
                                y = selection.y;
                                height = Event.current.mousePosition.y - selection.y;
                            }


                            var activeObjectIds = new List<string>();

                            var selectedRect = new Rect(x, y, width, height);


                            foreach(var node in graphGUI.Nodes)
                            {
                                var nodeRect = new Rect(node.GetRect());
                                nodeRect.x = nodeRect.x * NodeGUI.scaleFactor;
                                nodeRect.y = nodeRect.y * NodeGUI.scaleFactor;
                                nodeRect.width = nodeRect.width * NodeGUI.scaleFactor;
                                nodeRect.height = nodeRect.height * NodeGUI.scaleFactor;
                                // get containd nodes,
                                if(nodeRect.Overlaps(selectedRect))
                                {
                                    activeObjectIds.Add(node.Id);
                                }
                            }

                            foreach(var connection in graphGUI.Connections)
                            {
                                // get contained connection badge.
                                if(connection.GetRect().Overlaps(selectedRect))
                                {
                                    activeObjectIds.Add(connection.Id);
                                }
                            }

                            if(Event.current.shift)
                            {
                                // add current active object ids to new list.
                                foreach(var alreadySelectedObjectId in activeObject.idPosDict.ReadonlyDict().Keys)
                                {
                                    if(!activeObjectIds.Contains(alreadySelectedObjectId)) activeObjectIds.Add(alreadySelectedObjectId);
                                }
                            }
                            else
                            {
                                // do nothing, means cancel selections if nodes are not contained by selection.
                            }


                            Undo.RecordObject(this, "Select Objects");

                            activeObject = RenewActiveObject(activeObjectIds);
                            UpdateActivationOfObjects(activeObject);

                            selection = new AssetBundleGraphSelection(Vector2.zero);
                            modifyMode = ModifyMode.NONE;

                            HandleUtility.Repaint();
                            Event.current.Use();
                            break;
                        }

                    case ModifyMode.SCALING:
                        {
                            modifyMode = ModifyMode.NONE;
                            break;
                        }

                    case ModifyMode.SCROLLING:
                        {
                            modifyMode = ModifyMode.NONE;
                            break;
                        }
                    }
                    break;
                }
            }
        }

        public void OnEnable()
        {
            Init();
        }

        public void OnGUI()
        {
            DrawGUIToolBar();

            using(new EditorGUILayout.HorizontalScope())
            {
                DrawGUINodeGraph();
                if(showErrors)
                {
                    DrawGUINodeErrors();
                }
            }

            /*
				Event Handling:
				- Supporting dragging script into window to create node.
				- Context Menu	
				- NodeGUI connection.
				- Command(Delete, Copy, etc...)
			*/
            switch(Event.current.type)
            {
            // detect dragging script then change interface to "(+)" icon.
            case EventType.DragUpdated:
                {
                    var refs = DragAndDrop.objectReferences;

                    foreach(var refe in refs)
                    {
                        if(refe.GetType() == typeof(UnityEditor.MonoScript))
                        {
                            Type scriptTypeInfo = ((MonoScript)refe).GetClass();
                            Type inheritedTypeInfo = GetDragAndDropAcceptableScriptType(scriptTypeInfo);

                            if(inheritedTypeInfo != null)
                            {
                                // at least one asset is script. change interface.
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                break;
                            }
                        }
                    }
                    break;
                }

            // script drop on editor.
            case EventType.DragPerform:
                {
                    var pathAndRefs = new Dictionary<string, object>();
                    for(var i = 0; i < DragAndDrop.paths.Length; i++)
                    {
                        var path = DragAndDrop.paths[i];
                        var refe = DragAndDrop.objectReferences[i];
                        pathAndRefs[path] = refe;
                    }
                    var shouldSave = false;
                    foreach(var item in pathAndRefs)
                    {
                        var refe = (MonoScript)item.Value;
                        if(refe.GetType() == typeof(UnityEditor.MonoScript))
                        {
                            Type scriptTypeInfo = refe.GetClass();
                            Type inheritedTypeInfo = GetDragAndDropAcceptableScriptType(scriptTypeInfo);

                            if(inheritedTypeInfo != null)
                            {
                                var dropPos = Event.current.mousePosition;
                                var scriptName = refe.name;
                                var scriptClassName = scriptName;
                                AddNodeFromCode(scriptName, scriptClassName, inheritedTypeInfo, dropPos.x, dropPos.y);
                                shouldSave = true;
                            }
                        }
                    }

                    if(shouldSave)
                    {
                        SaveGraphThatRequiresReload();
                    }
                    break;
                }

            // show context menu
            case EventType.ContextClick:
                {
                    var rightClickPos = Event.current.mousePosition;
                    var menu = new GenericMenu();
                    foreach(var menuItemStr in AssetBundleGraphSettings.GUI_Menu_Item_TargetGUINodeDict.Keys)
                    {
                        var kind = AssetBundleGraphSettings.GUI_Menu_Item_TargetGUINodeDict[menuItemStr];
                        menu.AddItem(
                            new GUIContent(menuItemStr),
                            false,
                            () =>
                            {
                                AddNodeFromGUI(kind, rightClickPos.x, rightClickPos.y);
                                SaveGraphThatRequiresReload();
                                Repaint();
                            }
                        );
                    }
                    menu.ShowAsContext();
                    break;
                }

            /*
                Handling mouseUp at empty space. 
            */
            case EventType.MouseUp:
                {
                    modifyMode = ModifyMode.NONE;
                    HandleUtility.Repaint();

                    if(activeObject.idPosDict.ReadonlyDict().Any())
                    {
                        Undo.RecordObject(this, "Unselect");

                        foreach(var activeObjectId in activeObject.idPosDict.ReadonlyDict().Keys)
                        {
                            // unselect all.
                            foreach(var node in graphGUI.Nodes)
                            {
                                if(activeObjectId == node.Id)
                                {
                                    node.SetInactive();
                                }
                            }
                            foreach(var connection in graphGUI.Connections)
                            {
                                if(activeObjectId == connection.Id)
                                {
                                    connection.SetInactive();
                                }
                            }
                        }

                        activeObject = RenewActiveObject(new List<string>());

                    }

                    UpdateActivationOfObjects(activeObject);

                    break;
                }

            /*
                scale up or down by command & + or command & -.
            */
            case EventType.KeyDown:
                {
                    if(Event.current.command)
                    {
                        if(Event.current.shift && Event.current.keyCode == KeyCode.Semicolon)
                        {
                            NodeGUI.scaleFactor = NodeGUI.scaleFactor + 0.1f;
                            if(NodeGUI.scaleFactor < NodeGUI.SCALE_MIN) NodeGUI.scaleFactor = NodeGUI.SCALE_MIN;
                            if(NodeGUI.SCALE_MAX < NodeGUI.scaleFactor) NodeGUI.scaleFactor = NodeGUI.SCALE_MAX;
                            Event.current.Use();
                            break;
                        }

                        if(Event.current.keyCode == KeyCode.Minus)
                        {
                            NodeGUI.scaleFactor = NodeGUI.scaleFactor - 0.1f;
                            if(NodeGUI.scaleFactor < NodeGUI.SCALE_MIN) NodeGUI.scaleFactor = NodeGUI.SCALE_MIN;
                            if(NodeGUI.SCALE_MAX < NodeGUI.scaleFactor) NodeGUI.scaleFactor = NodeGUI.SCALE_MAX;
                            Event.current.Use();
                            break;
                        }
                    }
                    break;
                }

            case EventType.ValidateCommand:
                {
                    switch(Event.current.commandName)
                    {
                    // Delete active node or connection.
                    case "Delete":
                        {
                            if(activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                Event.current.Use();
                            }
                            break;
                        }

                    case "Copy":
                        {
                            if(activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                Event.current.Use();
                            }
                            break;
                        }

                    case "Cut":
                        {
                            if(activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                Event.current.Use();
                            }
                            break;
                        }

                    case "Paste":
                        {
                            if(copyField.datas == null)
                            {
                                break;
                            }

                            if(copyField.datas.Any())
                            {
                                Event.current.Use();
                            }
                            break;
                        }

                    case "SelectAll":
                        {
                            Event.current.Use();
                            break;
                        }

                    case "SoftDelete":
                        {
                            if(activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                Event.current.Use();
                            }
                            break;
                        }
                    }
                    break;
                }

            case EventType.ExecuteCommand:
                {
                    switch(Event.current.commandName)
                    {
                    // Delete active node or connection.
                    case "Delete":
                        {

                            if(!activeObject.idPosDict.ReadonlyDict().Any()) break;
                            Undo.RecordObject(this, "Delete Selection");

                            foreach(var targetId in activeObject.idPosDict.ReadonlyDict().Keys)
                            {
                                DeleteNode(targetId);
                                DeleteConnectionById(targetId);
                            }

                            SaveGraphThatRequiresReload();

                            activeObject = RenewActiveObject(new List<string>());
                            UpdateActivationOfObjects(activeObject);

                            Event.current.Use();
                            break;
                        }

                    case "Copy":
                        {
                            if(!activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                break;
                            }

                            Undo.RecordObject(this, "Copy Selection");

                            var targetNodeIds = activeObject.idPosDict.ReadonlyDict().Keys.ToList();
                            var targetNodeJsonRepresentations = JsonRepresentations(targetNodeIds);
                            copyField = new CopyField(targetNodeJsonRepresentations, CopyType.COPYTYPE_COPY);

                            Event.current.Use();
                            break;
                        }

                    case "Cut":
                        {
                            if(!activeObject.idPosDict.ReadonlyDict().Any())
                            {
                                break;
                            }

                            Undo.RecordObject(this, "Cut Selection");
                            var targetNodeIds = activeObject.idPosDict.ReadonlyDict().Keys.ToList();
                            var targetNodeJsonRepresentations = JsonRepresentations(targetNodeIds);
                            copyField = new CopyField(targetNodeJsonRepresentations, CopyType.COPYTYPE_CUT);

                            foreach(var targetId in activeObject.idPosDict.ReadonlyDict().Keys)
                            {
                                DeleteNode(targetId);
                                DeleteConnectionById(targetId);
                            }

                            SaveGraphThatRequiresReload();
                            InitializeGraph();

                            activeObject = RenewActiveObject(new List<string>());
                            UpdateActivationOfObjects(activeObject);

                            Event.current.Use();
                            break;
                        }

                    case "Paste":
                        {

                            if(copyField.datas == null)
                            {
                                break;
                            }

                            var nodeNames = graphGUI.Nodes.Select(node => node.Name).ToList();
                            var duplicatingData = new List<NodeGUI>();

                            if(copyField.datas.Any())
                            {
                                var pasteType = copyField.type;
                                foreach(var copyFieldData in copyField.datas)
                                {
                                    var nodeJsonDict = AssetBundleGraph.Json.Deserialize(copyFieldData) as Dictionary<string, object>;
                                    var pastingNode = new NodeGUI(new NodeData(nodeJsonDict));
                                    var pastingNodeName = pastingNode.Name;

                                    var nameOverlapping = nodeNames.Where(name => name == pastingNodeName).ToList();

                                    switch(pasteType)
                                    {
                                    case CopyType.COPYTYPE_COPY:
                                        {
                                            if(2 <= nameOverlapping.Count)
                                            {
                                                continue;
                                            }
                                            break;
                                        }
                                    case CopyType.COPYTYPE_CUT:
                                        {
                                            if(1 <= nameOverlapping.Count)
                                            {
                                                continue;
                                            }
                                            break;
                                        }
                                    }

                                    duplicatingData.Add(pastingNode);
                                }
                            }
                            // consume copyField
                            copyField.datas = null;

                            if(!duplicatingData.Any())
                            {
                                break;
                            }

                            Undo.RecordObject(this, "Paste");
                            foreach(var newNode in duplicatingData)
                            {
                                DuplicateNode(newNode);
                            }

                            SaveGraphThatRequiresReload();
                            InitializeGraph();

                            Event.current.Use();
                            break;
                        }

                    case "SelectAll":
                        {
                            Undo.RecordObject(this, "Select All Objects");

                            var selectionIds = graphGUI.Nodes.Select(node => node.Id).ToList();
                            selectionIds.AddRange(graphGUI.Connections.Select(con => con.Id).ToList());
                            activeObject = RenewActiveObject(selectionIds);

                            // select all.
                            foreach(var node in graphGUI.Nodes)
                            {
                                node.SetActive();
                            }
                            foreach(var connection in graphGUI.Connections)
                            {
                                connection.SetActive();
                            }

                            UpdateUnitySelection();

                            Event.current.Use();
                            break;
                        }

                    case "SoftDelete":
                        {
                            Undo.RecordObject(this, "Delete Selection");
                            foreach(var id in activeObject.idPosDict.ReadonlyDict().Keys)
                            {
                                DeleteNode(id);
                                DeleteConnectionById(id);
                            }

                            SaveGraphThatRequiresReload();

                            activeObject = RenewActiveObject(new List<string>());
                            UpdateActivationOfObjects(activeObject);
                            Event.current.Use();
                            break;
                        }

                    default:
                        {

                            break;
                        }
                    }
                    break;
                }
            }
        }

        private List<string> JsonRepresentations(List<string> nodeIds)
        {
            return graphGUI.Nodes.Where(nodeGui => nodeIds.Contains(nodeGui.Id)).Select(nodeGui => nodeGui.Data.ToJsonString()).ToList();
        }

        private Type GetDragAndDropAcceptableScriptType(Type type)
        {
            if(typeof(IPrefabBuilder).IsAssignableFrom(type) && !type.IsInterface && PrefabBuilderUtility.HasValidCustomPrefabBuilderAttribute(type))
            {
                return typeof(IPrefabBuilder);
            }
            if(typeof(IModifier).IsAssignableFrom(type) && !type.IsInterface && ModifierUtility.HasValidCustomModifierAttribute(type))
            {
                return typeof(IModifier);
            }
            if(typeof(IValidator).IsAssignableFrom(type) && !type.IsInterface && ValidatorUtility.HasValidCustomValidatorAttribute(type))
            {
                return typeof(IValidator);
            }
            return null;
        }

        private void AddNodeFromCode(string name, string scriptClassName, Type scriptBaseType, float x, float y)
        {
            NodeGUI newNode = null;

            if(scriptBaseType == typeof(IModifier))
            {
                var modifier = ModifierUtility.CreateModifier(scriptClassName);
                UnityEngine.Assertions.Assert.IsNotNull(modifier);

                newNode = new NodeGUI(new NodeData(name, NodeKind.MODIFIER_GUI, x, y));
                newNode.Data.ScriptClassName = scriptClassName;
                newNode.Data.InstanceData.DefaultValue = modifier.Serialize();
            }
            if(scriptBaseType == typeof(IPrefabBuilder))
            {
                var builder = PrefabBuilderUtility.CreatePrefabBuilderByClassName(scriptClassName);
                UnityEngine.Assertions.Assert.IsNotNull(builder);

                newNode = new NodeGUI(new NodeData(name, NodeKind.PREFABBUILDER_GUI, x, y));
                newNode.Data.ScriptClassName = scriptClassName;
                newNode.Data.InstanceData.DefaultValue = builder.Serialize();
            }
            if(scriptBaseType == typeof(IValidator))
            {
                var validator = ValidatorUtility.CreateValidator(scriptClassName);
                UnityEngine.Assertions.Assert.IsNotNull(validator);

                newNode = new NodeGUI(new NodeData(name, NodeKind.VALIDATOR_GUI, x, y));
                newNode.Data.ScriptClassName = scriptClassName;
                newNode.Data.InstanceData.DefaultValue = validator.Serialize();
            }


            if(newNode == null)
            {
                Debug.LogError("Could not add node from code. " + scriptClassName + "(base:" + scriptBaseType +
                    ") is not supported to create from code.");
                return;
            }

            AddNodeGUI(newNode);
        }

        private NodeGUI AddNodeFromGUI(NodeKind kind, float x, float y)
        {

            Undo.RecordObject(this, "Add " + AssetBundleGraphSettings.DEFAULT_NODE_NAME[kind] + " Node");

            var number = graphGUI.Nodes.Where(node => node.Kind == kind).ToList().Count;
            string nodeName = AssetBundleGraphSettings.DEFAULT_NODE_NAME[kind] + number;

            NodeGUI newNode = new NodeGUI(new NodeData(nodeName, kind, x, y));

            AddNodeGUI(newNode);

            if(kind == NodeKind.FILTER_GUI)
            {
                newNode.Data.AddFilterCondition("Textures", string.Empty, typeof(TextureImporter).ToString(), false);
                newNode.Data.AddFilterCondition("Models", string.Empty, typeof(ModelImporter).ToString(), false);
                newNode.Data.AddFilterCondition("Audio", string.Empty, typeof(AudioImporter).ToString(), false);
                newNode.UpdateNodeRect();
            }
            if(kind == NodeKind.WARP_IN)
            {
                string outNodeName = AssetBundleGraphSettings.DEFAULT_NODE_NAME[NodeKind.WARP_OUT] + number;
                NodeGUI outNode = new NodeGUI(new NodeData(outNodeName, NodeKind.WARP_OUT, x + 100, y));
                newNode.Data.RelatedNodeId = outNode.Id;
                outNode.Data.RelatedNodeId = newNode.Id;
                AddNodeGUI(outNode);
                AddConnection("warpConnection", newNode, newNode.Data.OutputPoints[0], outNode, outNode.Data.InputPoints[0]);
            }


            return newNode;
        }

        private void DrawStraightLineFromCurrentEventSourcePointTo(Vector2 to, NodeEvent eventSource)
        {
            if(eventSource == null)
            {
                return;
            }
            var p = eventSource.point.GetGlobalPosition(eventSource.eventSourceNode);
            Handles.DrawLine(new Vector3(p.x, p.y, 0f), new Vector3(to.x, to.y, 0f));
        }

        /**
		 * Handle Node Event
		*/
        private void HandleNodeEvent(NodeEvent e)
        {
            switch(modifyMode)
            {
            case ModifyMode.CONNECTING:
                {
                    switch(e.eventType)
                    {
                    /*
                        handling
                    */
                    case NodeEvent.EventType.EVENT_NODE_MOVING:
                        {
                            // do nothing.
                            break;
                        }

                    /*
                        connection drop detected from toward node.
                    */
                    case NodeEvent.EventType.EVENT_NODE_CONNECTION_RAISED:
                        {
                            // finish connecting mode.
                            modifyMode = ModifyMode.NONE;

                            if(currentEventSource == null)
                            {
                                break;
                            }

                            var sourceNode = currentEventSource.eventSourceNode;
                            var sourceConnectionPoint = currentEventSource.point;

                            var targetNode = e.eventSourceNode;
                            var targetConnectionPoint = e.point;

                            if(sourceNode.Id == targetNode.Id)
                            {
                                break;
                            }

                            if(!IsConnectablePointFromTo(sourceConnectionPoint, targetConnectionPoint))
                            {
                                break;
                            }

                            var startNode = sourceNode;
                            var startConnectionPoint = sourceConnectionPoint;
                            var endNode = targetNode;
                            var endConnectionPoint = targetConnectionPoint;

                            // reverse if connected from input to output.
                            if(sourceConnectionPoint.IsInput)
                            {
                                startNode = targetNode;
                                startConnectionPoint = targetConnectionPoint;
                                endNode = sourceNode;
                                endConnectionPoint = sourceConnectionPoint;
                            }

                            var outputPoint = startConnectionPoint;
                            var inputPoint = endConnectionPoint;
                            var label = startConnectionPoint.Label;

                            // if two nodes are not supposed to connect, dismiss
                            if(!ConnectionData.CanConnect(startNode.Data, endNode.Data))
                            {
                                break;
                            }

                            AddConnection(label, startNode, outputPoint, endNode, inputPoint);
                            SaveGraphThatRequiresReload();
                            break;
                        }

                    /*
                        connection drop detected by started node.
                    */
                    case NodeEvent.EventType.EVENT_NODE_CONNECTION_OVERED:
                        {
                            // finish connecting mode.
                            modifyMode = ModifyMode.NONE;

                            /*
                                connect when dropped target is connectable from start connectionPoint.
                            */
                            var node = FindNodeByPosition(e.globalMousePosition);
                            if(node == null)
                            {
                                break;
                            }

                            // ignore if target node is source itself.
                            if(node == e.eventSourceNode)
                            {
                                break;
                            }

                            var pointAtPosition = node.FindConnectionPointByPosition(e.globalMousePosition);
                            if(pointAtPosition == null)
                            {
                                break;
                            }

                            var sourcePoint = currentEventSource.point;

                            // limit by connectable or not.
                            if(!IsConnectablePointFromTo(sourcePoint, pointAtPosition))
                            {
                                break;
                            }

                            var isInput = currentEventSource.point.IsInput;
                            var startNode = (isInput) ? node : e.eventSourceNode;
                            var endNode = (isInput) ? e.eventSourceNode : node;
                            var startConnectionPoint = (isInput) ? pointAtPosition : currentEventSource.point;
                            var endConnectionPoint = (isInput) ? currentEventSource.point : pointAtPosition;
                            var outputPoint = startConnectionPoint;
                            var inputPoint = endConnectionPoint;
                            var label = startConnectionPoint.Label;

                            // if two nodes are not supposed to connect, dismiss
                            if(!ConnectionData.CanConnect(startNode.Data, endNode.Data))
                            {
                                break;
                            }

                            AddConnection(label, startNode, outputPoint, endNode, inputPoint);
                            SaveGraphThatRequiresReload();
                            break;
                        }

                    default:
                        {
                            modifyMode = ModifyMode.NONE;
                            break;
                        }
                    }
                    break;
                }
            case ModifyMode.NONE:
                {
                    switch(e.eventType)
                    {
                    /*
                        node move detected.
                    */
                    case NodeEvent.EventType.EVENT_NODE_MOVING:
                        {
                            var tappedNode = e.eventSourceNode;
                            var tappedNodeId = tappedNode.Id;

                            if(activeObject.idPosDict.ContainsKey(tappedNodeId))
                            {
                                // already active, do nothing for this node.
                                var distancePos = tappedNode.GetPos() - activeObject.idPosDict.ReadonlyDict()[tappedNodeId];

                                foreach(var node in graphGUI.Nodes)
                                {
                                    if(node.Id == tappedNodeId) continue;
                                    if(!activeObject.idPosDict.ContainsKey(node.Id)) continue;
                                    var relativePos = activeObject.idPosDict.ReadonlyDict()[node.Id] + distancePos;
                                    node.SetPos(relativePos);
                                }
                                break;
                            }

                            if(Event.current.shift)
                            {
                                Undo.RecordObject(this, "Select Objects");

                                var additiveIds = new List<string>(activeObject.idPosDict.ReadonlyDict().Keys);

                                additiveIds.Add(tappedNodeId);

                                activeObject = RenewActiveObject(additiveIds);

                                UpdateActivationOfObjects(activeObject);
                                UpdateSpacerRect();
                                break;
                            }

                            Undo.RecordObject(this, "Select " + tappedNode.Name);
                            activeObject = RenewActiveObject(new List<string> { tappedNodeId });
                            UpdateActivationOfObjects(activeObject);
                            break;
                        }

                    /*
                        start connection handling.
                    */
                    case NodeEvent.EventType.EVENT_NODE_CONNECT_STARTED:
                        {
                            modifyMode = ModifyMode.CONNECTING;
                            currentEventSource = e;
                            break;
                        }

                    case NodeEvent.EventType.EVENT_CLOSE_TAPPED:
                        {

                            Undo.RecordObject(this, "Delete Node");

                            var deletingNodeId = e.eventSourceNode.Id;
                            DeleteNode(deletingNodeId);

                            SaveGraphThatRequiresReload();
                            InitializeGraph();
                            break;
                        }

                    /*
                        releasse detected.
                            node move over.
                            node tapped.
                    */
                    case NodeEvent.EventType.EVENT_NODE_TOUCHED:
                        {
                            var movedNode = e.eventSourceNode;
                            var movedNodeId = movedNode.Id;

                            if(EditorApplication.timeSinceStartup - lastClickedTime < doubleClickTime)
                            {
                                movedNode.DoubleClickAction();
                                break;
                            }
                            lastClickedTime = EditorApplication.timeSinceStartup;

                            // already active, node(s) are just tapped or moved.
                            if(activeObject.idPosDict.ContainsKey(movedNodeId))
                            {

                                /*
                                    active nodes(contains tap released node) are possibly moved.
                                */
                                var movedIdPosDict = new Dictionary<string, Vector2>();
                                foreach(var node in graphGUI.Nodes)
                                {
                                    if(!activeObject.idPosDict.ContainsKey(node.Id)) continue;

                                    var startPos = activeObject.idPosDict.ReadonlyDict()[node.Id];
                                    if(node.GetPos() != startPos)
                                    {
                                        // moved.
                                        movedIdPosDict[node.Id] = node.GetPos();
                                    }
                                }

                                if(movedIdPosDict.Any())
                                {

                                    foreach(var node in graphGUI.Nodes)
                                    {
                                        if(activeObject.idPosDict.ReadonlyDict().Keys.Contains(node.Id))
                                        {
                                            var startPos = activeObject.idPosDict.ReadonlyDict()[node.Id];
                                            node.SetPos(startPos);
                                        }
                                    }

                                    Undo.RecordObject(this, "Move " + movedNode.Name);

                                    foreach(var node in graphGUI.Nodes)
                                    {
                                        if(movedIdPosDict.Keys.Contains(node.Id))
                                        {
                                            var endPos = movedIdPosDict[node.Id];
                                            node.SetPos(endPos);
                                        }
                                    }

                                    var activeObjectIds = activeObject.idPosDict.ReadonlyDict().Keys.ToList();
                                    activeObject = RenewActiveObject(activeObjectIds);
                                }
                                else
                                {
                                    List<string> activeIds = new List<string>(activeObject.idPosDict.ReadonlyDict().Keys);
                                    if(activeObject.idPosDict.ReadonlyDict().Count > 1)
                                    {
                                        // if there is a multiple selection, select only this node
                                        activeIds.RemoveAll(x => x != movedNodeId);
                                    }
                                    else
                                    {
                                        // if this is the only node in the selection, deselect it
                                        activeIds.Clear();
                                    }

                                    Undo.RecordObject(this, "Select Objects");

                                    activeObject = RenewActiveObject(activeIds);
                                    //break;
                                }

                                UpdateActivationOfObjects(activeObject);

                                UpdateSpacerRect();
                                SaveGraph();
                                break;
                            }

                            if(Event.current.shift)
                            {
                                Undo.RecordObject(this, "Select Objects");

                                var additiveIds = new List<string>(activeObject.idPosDict.ReadonlyDict().Keys);

                                // already contained, cancel.
                                if(additiveIds.Contains(movedNodeId))
                                {
                                    additiveIds.Remove(movedNodeId);
                                }
                                else
                                {
                                    additiveIds.Add(movedNodeId);
                                }

                                activeObject = RenewActiveObject(additiveIds);
                                UpdateActivationOfObjects(activeObject);

                                UpdateSpacerRect();
                                SaveGraph();
                                break;
                            }

                            Undo.RecordObject(this, "Select " + movedNode.Name);

                            activeObject = RenewActiveObject(new List<string> { movedNodeId });
                            UpdateActivationOfObjects(activeObject);


                            UpdateSpacerRect();
                            SaveGraph();
                            break;
                        }

                    default:
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            switch(e.eventType)
            {
            case NodeEvent.EventType.EVENT_CONNECTIONPOINT_ADDED:
                {
                    // adding point is handled by caller just repainting.
                    Repaint();
                    break;
                }

            case NodeEvent.EventType.EVENT_CONNECTIONPOINT_DELETED:
                {
                    // deleting point is handled by caller, so we are deleting connections associated with it.
                    graphGUI.Connections.RemoveAll(c => (c.InputPoint == e.point || c.OutputPoint == e.point));
                    Repaint();
                    break;
                }
            case NodeEvent.EventType.EVENT_CONNECTIONPOINT_LABELCHANGED:
                {
                    // point label change is handled by caller, so we are changing label of connection associated with it.
                    var affectingConnections = graphGUI.Connections.FindAll(c => c.OutputPoint == e.point);
                    affectingConnections.ForEach(c => c.Label = e.point.Label);
                    Repaint();
                    break;
                }
            case NodeEvent.EventType.EVENT_RECORDUNDO:
                {
                    Undo.RecordObject(this, e.message);
                    break;
                }
            case NodeEvent.EventType.EVENT_SAVE:
                {
                    SaveGraphThatRequiresReload(true);
                    Repaint();
                    break;
                }
            }
        }

        /**
			once expand, keep max size.
			it's convenience.
		*/
        private void UpdateSpacerRect()
        {
            var rightPoint = graphGUI.Nodes.OrderByDescending(node => node.GetRightPos()).Select(node => node.GetRightPos()).ToList()[0] + AssetBundleGraphSettings.WINDOW_SPAN;
            if(rightPoint < spacerRectRightBottom.x) rightPoint = spacerRectRightBottom.x;

            var bottomPoint = graphGUI.Nodes.OrderByDescending(node => node.GetBottomPos()).Select(node => node.GetBottomPos()).ToList()[0] + AssetBundleGraphSettings.WINDOW_SPAN;
            if(bottomPoint < spacerRectRightBottom.y) bottomPoint = spacerRectRightBottom.y;

            spacerRectRightBottom = new Vector2(rightPoint, bottomPoint);
        }

        public void DuplicateNode(NodeGUI node)
        {
            var newNode = node.Duplicate(
                node.GetX() + 10f,
                node.GetY() + 10f
            );
            AddNodeGUI(newNode);
        }

        private void AddNodeGUI(NodeGUI newNode)
        {

            int id = -1;

            foreach(var node in graphGUI.Nodes)
            {
                if(node.WindowId > id)
                {
                    id = node.WindowId;
                }
            }

            newNode.WindowId = id + 1;

            graphGUI.Nodes.Add(newNode);
        }

        public void DeleteNode(string deletingNodeId)
        {
            var deletedNodeIndex = graphGUI.Nodes.FindIndex(node => node.Id == deletingNodeId);
            if(0 <= deletedNodeIndex)
            {
                var node = graphGUI.Nodes[deletedNodeIndex];

                if(node.Data.Kind != NodeKind.IMPORTSETTING_GUI || EditorUtility.DisplayDialog("Delete " + node.Name, "Deleting the " + node.Name + " importer node will also delete the placeholder asset for config, are you sure?", "Delete", "Cancel"))
                {
                    node.SetInactive();
                    graphGUI.Nodes.RemoveAt(deletedNodeIndex);

                    if(node.Kind == NodeKind.WARP_IN || node.Kind == NodeKind.WARP_OUT)
                    {
                        DeleteNode(node.Data.RelatedNodeId);
                    }
                    if(node.Data.Kind == NodeKind.IMPORTSETTING_GUI)
                    {
                        IntegratedGUIImportSetting.RemoveConfigFile(node.Data.Id);
                    }
                }
            }
        }

        public void HandleConnectionEvent(ConnectionEvent e)
        {
            switch(modifyMode)
            {
            case ModifyMode.NONE:
                {
                    switch(e.eventType)
                    {

                    case ConnectionEvent.EventType.EVENT_CONNECTION_TAPPED:
                        {

                            if(Event.current.shift)
                            {
                                Undo.RecordObject(this, "Select Objects");

                                var objectId = string.Empty;

                                if(e.eventSourceCon != null)
                                {
                                    objectId = e.eventSourceCon.Id;
                                    if(!activeObject.idPosDict.ReadonlyDict().Any())
                                    {
                                        activeObject = RenewActiveObject(new List<string> { objectId });
                                    }
                                    else
                                    {
                                        var additiveIds = new List<string>(activeObject.idPosDict.ReadonlyDict().Keys);

                                        // already contained, cancel.
                                        if(additiveIds.Contains(objectId))
                                        {
                                            additiveIds.Remove(objectId);
                                        }
                                        else
                                        {
                                            additiveIds.Add(objectId);
                                        }

                                        activeObject = RenewActiveObject(additiveIds);
                                    }
                                }

                                UpdateActivationOfObjects(activeObject);
                                break;
                            }


                            Undo.RecordObject(this, "Select Connection");

                            var tappedConnectionId = e.eventSourceCon.Id;
                            foreach(var con in graphGUI.Connections)
                            {
                                if(con.Id == tappedConnectionId)
                                {
                                    con.SetActive();
                                    Selection.activeObject = con.ConnectionInspectorHelper;
                                    activeObject = RenewActiveObject(new List<string> { con.Id });
                                }
                                else
                                {
                                    con.SetInactive();
                                }
                            }

                            // set deactive for all nodes.
                            foreach(var node in graphGUI.Nodes)
                            {
                                node.SetInactive();
                            }
                            break;
                        }
                    case ConnectionEvent.EventType.EVENT_CONNECTION_DELETED:
                        {
                            Undo.RecordObject(this, "Delete Connection");

                            var deletedConnectionId = e.eventSourceCon.Id;

                            DeleteConnectionById(deletedConnectionId);

                            SaveGraphThatRequiresReload();
                            Repaint();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                    }
                    break;
                }
            }
        }

        public static void OpenAndCreateLoader(string path)
        {
            var window = GetWindow<AssetBundleGraphEditorWindow>();
            var node = window.AddNodeFromGUI(NodeKind.LOADER_GUI, 50, 50);
            var subpath = path.Replace("Assets/", "");
            node.Name = subpath;
            node.Data.LoaderLoadPath[BuildTargetUtility.DefaultTarget] = subpath;
            node.UpdateNodeRect();
            window.SaveGraphThatRequiresReload();
            window.Repaint();
            var ids = new List<string>();
            ids.Add(node.Id);
            window.activeObject = window.RenewActiveObject(ids);
            window.UpdateActivationOfObjects(window.activeObject);
            window.scrollPos = new Vector2(0, 0);

        }

        public static void OpenAndRunSelected(string[] nodeIds)
        {
            SelectAllRelatedTree(nodeIds.ToArray(), true);

            var window = GetWindow<AssetBundleGraphEditorWindow>();

            window.Run(window.ActiveBuildTarget, new List<string>(nodeIds));
        }

        public static void SelectAllRelatedTree(string[] nodeIds, bool includeWarps = true)
        {
            var window = GetWindow<AssetBundleGraphEditorWindow>();
            window.InitializeGraph();

            var rootNodes = window.graphGUI.Nodes.FindAll(x => nodeIds.Contains(x.Id));
            var subGraph = window.graphGUI.GetSubGraph(rootNodes.ToArray(), includeWarps);
            Vector2 upperLeft = new Vector2(float.MaxValue, float.MaxValue);

            List<string> ids = new List<string>();
            foreach(NodeGUI nodeGUI in subGraph.Nodes)
            {
                upperLeft.x = Mathf.Min(upperLeft.x, nodeGUI.Data.X);
                upperLeft.y = Mathf.Min(upperLeft.y, nodeGUI.Data.Y);

                ids.Add(nodeGUI.Id);
            }
            ids.AddRange(subGraph.Connections.ConvertAll(x => x.Id));

            window.activeObject = window.RenewActiveObject(ids);
            window.UpdateActivationOfObjects(window.activeObject);
            window.scrollPos = new Vector2(upperLeft.x - window.position.width * 0.4f, upperLeft.y - window.position.height * 0.4f);
        }

        public static void SelectNodeById(string nodeId)
        {
            var window = GetWindow<AssetBundleGraphEditorWindow>();
            var ids = new List<string>();
            ids.Add(nodeId);
            window.activeObject = window.RenewActiveObject(ids);
            window.UpdateActivationOfObjects(window.activeObject);
            var node = window.graphGUI.Nodes.Find(x => x.Id == nodeId);
            window.scrollPos = new Vector2(node.Data.X - window.position.width * 0.4f, node.Data.Y - window.position.height * 0.4f);
        }

        public static void ChangeNodeName(string nodeId, string newName)
        {
            var window = GetWindow<AssetBundleGraphEditorWindow>();
            window.graphGUI.Nodes.Find(x => x.Id == nodeId).Name = newName;
        }

        private void UpdateActivationOfObjects(ActiveObject currentActiveObject)
        {
            foreach(var node in graphGUI.Nodes)
            {
                if(currentActiveObject.idPosDict.ContainsKey(node.Id))
                {
                    node.SetActive();
                    continue;
                }

                node.SetInactive();
            }

            foreach(var connection in graphGUI.Connections)
            {
                if(currentActiveObject.idPosDict.ContainsKey(connection.Id))
                {
                    connection.SetActive();
                    continue;
                }

                connection.SetInactive();
            }

            var readOnlyDict = currentActiveObject.idPosDict.ReadonlyDict();
            if(readOnlyDict.Count == 1)
            {
                var node = graphGUI.Nodes.Find(x => readOnlyDict.Keys.First() == x.Id);
                if(node != null && (node.Data.Kind == NodeKind.WARP_IN || node.Data.Kind == NodeKind.WARP_OUT))
                {
                    graphGUI.Nodes.Find(x => x.Id == node.Data.RelatedNodeId).SetHighlighted();
                }
            }

            UpdateUnitySelection();

        }

        private void UpdateUnitySelection()
        {
            List<UnityEngine.Object> activeObjs = new List<UnityEngine.Object>();

            activeObjs.AddRange(graphGUI.Nodes.FindAll(x => x.NodeInspectorHelper.isActive).ConvertAll(x => x.NodeInspectorHelper).ToArray());
            activeObjs.AddRange(graphGUI.Connections.FindAll(x => x.ConnectionInspectorHelper.isActive).ConvertAll(x => x.ConnectionInspectorHelper).ToArray());

            Selection.objects = activeObjs.ToArray();
        }

        /**
			create new connection if same relationship is not exist yet.
		*/
        private void AddConnection(string label, NodeGUI startNode, ConnectionPointData startPoint, NodeGUI endNode, ConnectionPointData endPoint)
        {
            Undo.RecordObject(this, "Add Connection");

            var connectionsFromThisNode = graphGUI.Connections
                .Where(con => con.OutputNodeId == startNode.Id)
                .Where(con => con.OutputPoint == startPoint)
                .ToList();
            if(connectionsFromThisNode.Any())
            {
                var alreadyExistConnection = connectionsFromThisNode[0];
                DeleteConnectionById(alreadyExistConnection.Id);
            }

            if(!graphGUI.Connections.ContainsConnection(startPoint, endPoint))
            {
                graphGUI.Connections.Add(ConnectionGUI.CreateConnection(label, startPoint, endPoint));
            }
        }

        private NodeGUI FindNodeByPosition(Vector2 globalPos)
        {
            return graphGUI.Nodes.Find(n => n.Contains(globalPos));
        }

        private bool IsConnectablePointFromTo(ConnectionPointData sourcePoint, ConnectionPointData destPoint)
        {
            if(sourcePoint.IsInput)
            {
                return destPoint.IsOutput;
            }
            else
            {
                return destPoint.IsInput;
            }
        }

        private void DeleteConnectionById(string id)
        {
            var deletedConnectionIndex = graphGUI.Connections.FindIndex(con => con.Id == id);
            if(0 <= deletedConnectionIndex)
            {
                graphGUI.Connections[deletedConnectionIndex].SetInactive();
                graphGUI.Connections.RemoveAt(deletedConnectionIndex);
            }
        }

        public int GetUnusedWindowId()
        {
            int highest = 0;
            graphGUI.Nodes.ForEach((NodeGUI n) => { if(n.WindowId > highest) highest = n.WindowId; });
            return highest + 1;
        }
    }
}
