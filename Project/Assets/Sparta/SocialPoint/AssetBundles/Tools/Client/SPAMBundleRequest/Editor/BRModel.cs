using UnityEngine;
using UnityEditor;
using SocialPoint.Tool.Shared.TLGUI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using SocialPoint.Attributes;
using SocialPointEditor.Assets.PlatformEx;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class BRModel : TLModel
    {
        //To prevent calling when threaded
        static readonly string CurrentDataPath = Application.dataPath;
        static readonly string BDATA_PATH = BMDataAccessor.Paths.BundleDataPath;

		public string projectName { get { return brResponse.data.project_name; } }
        /// <summary>
        /// The bundle response with the project versions and versioning
        /// </summary>
        public BRResponse brResponse;

        public BRCompResultsResponse compilationsResponse;

        /// <summary>
        /// Bundle data dictionary(accessed by unique name) by project version
        /// </summary>
        public Dictionary<string, Dictionary<string, BundleData>> bdataDict;

        public bool IsDataLoaded { get; private set; }

        //logger storage
        List<string> _searchPaths = new List<string> ();

        public void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            IsDataLoaded = false;
        }

        void Init()
        {
            bdataDict = new Dictionary<string, Dictionary<string, BundleData>> ();
            string[] availableProjectPaths = brResponse.data.tagged_project_versions.Select(x => x.Value.project_path).ToArray();
			//If a single active project, use the current projectPath
            if(brResponse.VersionNames.Length == 1)
            {
                _searchPaths.Clear(); //error log purposes

                var projectVersion = brResponse.VersionNames[0];
                var bdataPath = Path.Combine(CurrentDataPath, BDATA_PATH.Substring("Assets/".Length)).ToSysPath();
                LoadBundlesFromFile(projectVersion, bdataPath);
            }
            else
            {
                foreach(var projectVersion in brResponse.VersionNames)
                {
                    _searchPaths.Clear(); //error log purposes
                    
                    var projectPath = brResponse.data.tagged_project_versions[projectVersion].project_path;
                    var bdataPath = GetFullBundleDataFilePath(projectPath, availableProjectPaths);
                    if (bdataPath == null)
                    {
                        throw new Exception(String.Format("No bundle data file found for projectPath '{0}'. Search paths:\n{1}", projectPath, String.Join("\n",_searchPaths.ToArray())));
                    }
                    LoadBundlesFromFile(projectVersion, bdataPath);
                }
            }

            _responseLock = new System.Object ();
            if(_compilationResults == null)
                _compilationResults = new List<BRCompilationResult> ();
            
            IsDataLoaded = true;
        }
        
        public void LoadCompilationResults(Attr response)
        {
            compilationsResponse = BRCompResultsResponse.FromAttr(response.AsDic["response"]);

            if (_compilationResults == null)
                _compilationResults = new List<BRCompilationResult>();
            else
                _compilationResults.Clear();

            foreach(BRCompResultsResponse.CompilationResultData data in compilationsResponse.compilations)
            {
                BRCompilationResult.CompilationState state = BRCompilationResult.CompilationState.PENDING;
                try {
                    state = (BRCompilationResult.CompilationState)Enum.Parse(typeof(BRCompilationResult.CompilationState), data.status, true);
                }catch(ArgumentException)
                {
                    Debug.LogWarning("Status of compilation result " + data.id + " is unknown");
                }

                BRCompilationResult compRes = new BRCompilationResult(data.id, state, data.created);

                compRes.author = data.author;

                foreach(List<object> list in data.bundles)
                {
                    compRes.Bundles.Add((string)list[0]);
                }

                _compilationResults.Add(compRes);
            }
        }

        public void InitFromResponse(Attr response)
        {
            brResponse = BRResponse.FromAttr(response.AsDic["response"].AsDic["versioning"]);
            Init();
        }

        public bool InitFromCache()
        {
            brResponse = BRResponse.Instance();
            if(brResponse.IsCached)
            {
                Init();
            }

            return brResponse.IsCached;
        }

        void LoadBundlesFromFile(string projectVersion, string bdataPath)
        {
            if (!File.Exists(bdataPath))
            {
                throw new Exception(String.Format("No bundle data file found at path '{0}'", bdataPath));
            }

            var jsonContent = File.ReadAllText(bdataPath);
            BundleData[] bundles = JsonMapper.ToObject<BundleData[]>(jsonContent);
            bdataDict[projectVersion] = new Dictionary<string, BundleData> ();

            for(int i = 0; i < bundles.Length; ++i)
            {
                bdataDict[projectVersion][bundles[i].name] = bundles[i];
            }
        }

		/// <summary>
		/// Gets the full BundleData.txt file path.
		/// </summary>
		/// <returns>The full bundle data file path.</returns>
		/// <param name="projectFolder">Project folder relative to the svn root (ie. 'trunk', 'branch/v1.3.1').</param>
		/// <param name="availableFolders">List of available project folders to search (ie. 'trunk', 'branch/v1.3.1').</param>
		string GetFullBundleDataFilePath(string projectFolder, string[] availableFolders)
		{
            if (!availableFolders.Contains(projectFolder))
			{
                _searchPaths.AddRange(availableFolders);
                return null;
			}

            var currDataPath = CurrentDataPath;
            _searchPaths.Add(currDataPath  + " <- " + projectFolder);
			if (currDataPath.Contains(projectFolder))
			{
                return Path.Combine(currDataPath, BDATA_PATH).ToSysPath();
			}

            foreach(var availableFolder in availableFolders)
			{
                _searchPaths.Add(currDataPath + " <- " + availableFolder);
				if (currDataPath.Contains(availableFolder))
				{
					return Path.Combine(currDataPath.Replace(availableFolder, projectFolder), BDATA_PATH).ToSysPath();
				}
			}

			return null;
		}

        // CompilationResult storage

        private System.Object               _responseLock = new System.Object ();

        List<BRCompilationResult>            _compilationResults;
        public List<BRCompilationResult>     compilationResults
        {
            get
            {
                lock(_responseLock)
                {
                    return _compilationResults;
                }
            }
        }

        public List<BRCompilationResult> PendingCompilations
        {
            get
            {
                lock(_responseLock)
                {
                    return _compilationResults.Where(x => x.State == BRCompilationResult.CompilationState.PENDING).ToList();
                }
            }
        }

        public bool AddCompilationResult (BRCompilationResult compilationResult)
        {
            lock(_responseLock)
            {
                var existingCompilation = _compilationResults.Find((BRCompilationResult obj) => obj.Equals(compilationResult));
                if (existingCompilation == null)
                {
                    _compilationResults.Add(compilationResult);
                    _compilationResults.Sort(BRCompilationResult.SortByDate);
                    
                    //hard limit of stored compilation results by date. 1000.
                    if (_compilationResults.Count > 1000)
                        _compilationResults.RemoveAt(_compilationResults.Count - 1);
                    
                    return true;
                }
                return false;
            }
        }
        
        public bool CompleteCompilationResult (int compilationId, BRCompilationResult.CompilationState state)
        {
            lock(_responseLock)
            {
                var existingCompilation = _compilationResults.Find((BRCompilationResult obj) => obj.Id.Equals(compilationId));
                if (existingCompilation != null && existingCompilation.State != state)
                {
                    existingCompilation.State = state;
                    return true;
                }
                return false;
            }
        }

        //
	}
}
