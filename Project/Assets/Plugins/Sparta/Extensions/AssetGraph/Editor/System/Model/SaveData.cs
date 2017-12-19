using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssetBundleGraph
{

    class SaveDataConstants
    {
        /*
			data key for AssetBundleGraph.json
		*/

        public const string GROUPING_KEYWORD_DEFAULT = "/Group_*/";
        public const string BUNDLECONFIG_BUNDLENAME_TEMPLATE_DEFAULT = "*";

        // by default, AssetBundleGraph's node has only 1 InputPoint. and 
        // this is only one definition of it's label.
        public const string DEFAULT_INPUTPOINT_LABEL = "-";
        public const string DEFAULT_OUTPUTPOINT_LABEL = "+";
        public const string BUNDLECONFIG_BUNDLE_OUTPUTPOINT_LABEL = "bundles";
        public const string BUNDLECONFIG_VARIANTNAME_DEFAULT = "";

        public const string DEFAULT_FILTER_KEYWORD = "keyword";
        public const string DEFAULT_FILTER_KEYTYPE = "Any";

        public const string FILTER_KEYWORD_WILDCARD = "*";

        public const string NODE_INPUTPOINT_FIXED_LABEL = "FIXED_INPUTPOINT_ID";
    }

    /*
	 * Json save data which holds all AssetBundleGraph settings and configurations.
	 */
    public class SaveData
    {
        public const string LASTMODIFIED = "lastModified";
        public const string NODES = "nodes";
        public const string CONNECTIONS = "connections";

        private Dictionary<string, object> m_jsonData;

        private Graph m_graph;
        private DateTime m_lastModified;

        private LoaderSaveData loaderSaveData = new LoaderSaveData();

        public SaveData()
        {
            m_lastModified = DateTime.UtcNow;
            m_graph = new Graph();
        }

        public SaveData(Dictionary<string, object> jsonData)
        {
            m_jsonData = jsonData;
            m_graph = new Graph();

            m_lastModified = Convert.ToDateTime(m_jsonData[LASTMODIFIED] as string);

            var nodeList = m_jsonData[NODES] as List<object>;
            var connList = m_jsonData[CONNECTIONS] as List<object>;

            foreach(var n in nodeList)
            {
                m_graph.Nodes.Add(new NodeData(n as Dictionary<string, object>));
            }

            foreach(var c in connList)
            {
                m_graph.Connections.Add(new ConnectionData(c as Dictionary<string, object>));
            }

        }

        public SaveData(List<NodeGUI> nodes, List<ConnectionGUI> connections)
        {
            m_jsonData = null;

            m_lastModified = DateTime.UtcNow;
            m_graph = new Graph(nodes, connections);
        }

        public DateTime LastModified
        {
            get
            {
                return m_lastModified;
            }
        }

        public Graph Graph
        {
            get
            {
                return m_graph;
            }
        }

        private Dictionary<string, object> ToJsonDictionary()
        {

            var nodeList = new List<Dictionary<string, object>>();
            var connList = new List<Dictionary<string, object>>();

            foreach(NodeData n in m_graph.Nodes)
            {
                nodeList.Add(n.ToJsonDictionary());
            }

            foreach(ConnectionData c in m_graph.Connections)
            {
                connList.Add(c.ToJsonDictionary());
            }

            return new Dictionary<string, object>{
                {LASTMODIFIED, m_lastModified.ToString()},
                {NODES, nodeList},
                {CONNECTIONS, connList}
            };
        }


        //
        // Save/Load to disk
        //

        public static string SaveDataDirectoryPath
        {
            get
            {
                return FileUtility.PathCombine(Application.dataPath, AssetBundleGraphSettings.ASSETNBUNDLEGRAPH_DATA_PATH);
            }
        }

        private static string SaveDataPath
        {
            get
            {
                return FileUtility.PathCombine(SaveDataDirectoryPath, AssetBundleGraphSettings.ASSETBUNDLEGRAPH_DATA_NAME);
            }
        }


        public void Save()
        {
            var dir = SaveDataDirectoryPath;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            m_lastModified = DateTime.UtcNow;

            var dataStr = Json.Serialize(ToJsonDictionary());
            var prettified = Json.Prettify(dataStr);

            using(var sw = new StreamWriter(SaveDataPath))
            {
                sw.Write(prettified);
            }

            loaderSaveData.UpdateLoaderData(m_graph.CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI));
            loaderSaveData.Save();

            // reflect change of data.
            //AssetDatabase.Refresh(); COMMENTED FOR PERFORMANCE PURPOSES, NO DRAWBACK FOUND.
        }

        private Dictionary<string, object> ToJsonRootNodes()
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            var rootNodes = m_graph.CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI && x.LoaderLoadPath != null);

            foreach(var loaderNode in rootNodes)
            {
                res.Add(loaderNode.Id, loaderNode.LoaderLoadPath.ToJsonDictionary());
            }

            return res;
        }

        public static bool IsSaveDataAvailableAtDisk()
        {
            return File.Exists(SaveDataPath);
        }



        private static SaveData Load()
        {
            var dataStr = string.Empty;
            using(var sr = new StreamReader(SaveDataPath))
            {
                dataStr = sr.ReadToEnd();
            }
            var deserialized = AssetBundleGraph.Json.Deserialize(dataStr) as Dictionary<string, object>;
            return new SaveData(deserialized);
        }

        public static SaveData RecreateDataOnDisk()
        {
            SaveData newSaveData = new SaveData();
            newSaveData.Save();
            AssetDatabase.Refresh();
            return newSaveData;
        }

        public static SaveData LoadFromDisk()
        {

            if(!IsSaveDataAvailableAtDisk())
            {
                return RecreateDataOnDisk();
            }

            try
            {
                SaveData saveData = Load();
                if(!saveData.Validate())
                {
                    saveData.Save();

                    // reload and construct again from disk
                    return Load();
                }
                else
                {
                    return saveData;
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to deserialize AssetBundleGraph settings. Error:" + e + " File:" + SaveDataPath);
            }

            return new SaveData();
        }

        /*
		 * Checks deserialized SaveData, and make some changes if necessary
		 * return false if any changes are perfomed.
		 */
        public bool Validate()
        {
            var changed = false;

            List<NodeData> removingNodes = new List<NodeData>();
            List<ConnectionData> removingConnections = new List<ConnectionData>();

            /*
				delete undetectable node.
			*/
            foreach(var n in m_graph.Nodes)
            {
                if(!n.Validate(m_graph.Nodes, m_graph.Connections))
                {
                    removingNodes.Add(n);
                    changed = true;
                }
            }

            foreach(var c in m_graph.Connections)
            {
                if(!c.Validate(m_graph.Nodes, m_graph.Connections))
                {
                    removingConnections.Add(c);
                    changed = true;
                }
            }

            if(changed)
            {
                m_graph.Nodes.RemoveAll(n => removingNodes.Contains(n));
                m_graph.Connections.RemoveAll(c => removingConnections.Contains(c));
                m_lastModified = DateTime.UtcNow;
            }

            return !changed;
        }
    }

    public class LoaderSaveData
    {
        private const string LOADER_SAVE_DATA = "loaders";
        public class LoaderData
        {
            private const string LOADER_ID = "id";
            private const string LOADER_PATH = "path";
            private const string LOADER_PREPROCESS = "preprocess";
            private const string LOADER_PERMANENT = "permanent";

            public string id;
            public SerializableMultiTargetString paths;
            public bool isPreProcess;
            public bool isPermanent;


            public LoaderData(string id, SerializableMultiTargetString paths, bool isPreProcess, bool isPermanent)
            {
                this.id = id;
                this.paths = paths;
                this.isPreProcess = isPreProcess;
                this.isPermanent = isPermanent;
            }

            public LoaderData(Dictionary<string, object> rawData)
            {
                this.id = rawData[LOADER_ID] as string;
                this.paths = new SerializableMultiTargetString(rawData[LOADER_PATH] as Dictionary<string, object>);
                if(rawData.ContainsKey(LOADER_PREPROCESS))
                {
                    this.isPreProcess = Convert.ToBoolean(rawData[LOADER_PREPROCESS]);
                }
                if(rawData.ContainsKey(LOADER_PERMANENT))
                {
                    this.isPermanent = Convert.ToBoolean(rawData[LOADER_PERMANENT]);
                }
            }

            public Dictionary<string, object> ToJsonDictionary()
            {
                Dictionary<string, object> jsonDict = new Dictionary<string, object>();

                jsonDict.Add(LOADER_ID, id);
                jsonDict.Add(LOADER_PATH, paths.ToJsonDictionary());
                jsonDict.Add(LOADER_PREPROCESS, isPreProcess);
                jsonDict.Add(LOADER_PERMANENT, isPermanent);

                return jsonDict;
            }
        }

        public static bool isDirty = true;
        public static event Action OnSave;

        private List<LoaderData> loaders;
        public List<LoaderData> LoaderPaths
        {
            get
            {
                return loaders;
            }
        }

        private static string LoaderSaveDataPath
        {
            get
            {
                return FileUtility.PathCombine(SaveData.SaveDataDirectoryPath, AssetBundleGraphSettings.ASSETBUNDLEGRAPH_LOADER_DATA_NAME);
            }
        }

        public LoaderSaveData()
        {
            loaders = new List<LoaderData>();
        }


        public LoaderSaveData(Graph graph)
        {
            var fullLoaders = graph.CollectAllNodes(x => x.Kind == NodeKind.LOADER_GUI);
            UpdateLoaderData(fullLoaders);
        }

        public LoaderSaveData(Dictionary<string, object> rawData)
        {
            var rawLoaders = rawData[LOADER_SAVE_DATA] as List<object>;
            loaders = new List<LoaderData>();

            foreach(var rawLoader in rawLoaders)
            {
                loaders.Add(new LoaderData(rawLoader as Dictionary<string, object>));
            }
        }

        public void UpdateLoaderData(List<NodeData> fullLoaders)
        {
            loaders = fullLoaders.ConvertAll(x => new LoaderData(x.Id, x.LoaderLoadPath, x.PreProcess, x.Permanent));
        }

        public void Save()
        {
            var dir = SaveData.SaveDataDirectoryPath;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var serializedData = Json.Serialize(ToJsonDictionary());
            var loaderPrettyfied = Json.Prettify(serializedData);

            using(var sw = new StreamWriter(LoaderSaveDataPath))
            {
                sw.Write(loaderPrettyfied);
            }

            if(OnSave != null)
                OnSave();
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            List<Dictionary<string, object>> loadersData = new List<Dictionary<string, object>>();

            foreach(var loader in loaders)
            {
                loadersData.Add(loader.ToJsonDictionary());
            }

            dictionary.Add(LOADER_SAVE_DATA, loadersData);

            return dictionary;
        }

        /// <summary>
        /// Finds the best suitable loader for the provided asset path
        /// </summary>
        /// <param name="path">Path of the asset</param>
        /// <returns>LoaderData of the nearest LoaderFolder, null if none are suitable</returns>
        public LoaderData GetBestLoaderData(string assetPath)
        {
            LoaderData res = null;
            var path = Path.HasExtension(assetPath) ? assetPath : assetPath + "/";
            var separator = "Assets/";
            path = path.Remove(0, path.IndexOf(separator) + separator.Length);

            if(path.Contains(AssetBundleGraphSettings.ASSETBUNDLEGRAPH_PATH.Remove(0, separator.Length)))
            {
                return res;
            }

            foreach(LoaderData dataPath in loaders)
            {
                if(dataPath.paths.CurrentPlatformValue == string.Empty || path.StartsWith(dataPath.paths.CurrentPlatformValue + "/"))
                {
                    if(res == null || res.paths.CurrentPlatformValue.Length < dataPath.paths.CurrentPlatformValue.Length)
                    {
                        res = dataPath;
                    }
                }
            }

            return res;
        }

        public static LoaderSaveData RecreateDataOnDisk()
        {
            LoaderSaveData lSaveData = new LoaderSaveData();
            lSaveData.Save();
            AssetDatabase.Refresh();
            return lSaveData;
        }

        public static LoaderSaveData LoadFromDisk()
        {
            if(!IsLoaderDataAvailableAtDisk())
            {
                return RecreateDataOnDisk();
            }

            try
            {
                var dataStr = string.Empty;
                using(var sr = new StreamReader(LoaderSaveDataPath))
                {
                    dataStr = sr.ReadToEnd();
                }
                var deserialized = Json.Deserialize(dataStr) as Dictionary<string, object>;

                return new LoaderSaveData(deserialized);
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to deserialize AssetBundleGraph settings. Error:" + e + " File:" + LoaderSaveDataPath);
            }

            return new LoaderSaveData();
        }

        public static bool IsLoaderDataAvailableAtDisk()
        {
            return File.Exists(LoaderSaveDataPath);
        }
    }
}
