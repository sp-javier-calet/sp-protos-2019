using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Tool.Shared;
using SocialPoint.Tool.Shared.TLGUI;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class BRController : TLController
    {
        public static readonly long MIN_TASK_STATUS_POLL_TIME = 5000;

        public BRView  View     { get { return (BRView)_view; } }
        public BRModel Model    { get { return (BRModel)_model; } }

        public SPAMAuthenticator auth;
        public SPAMApiBundleRequest _api;
        long _lastCompilationPollTime;
        bool areRequestsPending = false;

        /// <summary>
        /// Gets the current selected version of the project.
        /// </summary>
        /// <value>The current version.</value>
        public string CurrentVersion { get { return View.cbVersionSelector.Selected; } }

        TLEvent<Attr> versioningReceivedEvent;
        TLEvent<Attr> prevCompilationsReceivedEvent;
        TLEvent newCompilationResultCreated;

        public BRController(TLView view, TLModel model) : base(view, model)
        {
            Init();
        }

        void Init()
        {
            _lastCompilationPollTime = 0;
            versioningReceivedEvent = new TLEvent<Attr> ("VersioningReceivedEvent");
            prevCompilationsReceivedEvent = new TLEvent<Attr>("PrevCompilationsReceivedEvent");
            newCompilationResultCreated = new TLEvent ("NewCompilationResultCreated");

            View.cbVersionSelector.selectionChange.Connect(OnVersionChange);
            //Hack to prevent input swallowed by widgets under the combo box selector
            View.cbVersionSelector.expanded.Connect(View.twBundleSelector.SetDisabled);

            View.ckSplitEnabled.onCheckEvent.Connect(OnCheckChanged);
            View.btRequestBundles.onClickEvent.Connect(OnRequestBundlesButton);
            View.btLogout.onClickEvent.Connect(OnLogoutButton);

            versioningReceivedEvent.Connect(OnVersioningResponse);
            prevCompilationsReceivedEvent.Connect(OnPreviousRequestsResponded);
            newCompilationResultCreated.Connect(OnCompilationResultCreated);
        }

        public override void OnLoad()
        {
            if(Model.IsDataLoaded)
            {
                //Config view
                //Title
                View.lbTitle.text = Model.projectName;
                View.lbSplitThreshold.text = string.Format("Auto-Split threshold: {0}", SPAMConfigJson.RefreshInstance().maxBundlesPerRequest);
                View.ckSplitEnabled.SetCheck(SPAMConfigJson.Instance.autoSplitEnabled);

                //Combobox
                View.cbVersionSelector.Add(Model.bdataDict.Keys.ToArray());

                //Try to set 'trunk' by default if it exists
                View.cbVersionSelector.SetSelected("trunk");
            }
            else
            {
                View.lbTitle.text = "Log in";
            }
        }

        /// <summary>
        /// Used when the data model changes or initializes and needs to refresh the view
        /// </summary>
        void Reload()
        {
            OnLoad();
        }

        public void OnAuthentication(SPAMAuthenticator _auth)
        {
            auth = _auth;
            _api = new SPAMApiBundleRequest();
            View.btLogout.SetDisabled(false);


            //Is versioning cached ?
            var cachedVersioning = BRResponse.Instance();

            var cachedTs = cachedVersioning.IsCached ? cachedVersioning.version_timestamp : -1;

			//Found an invalid "Dragon S.", cannot rely on PlayerSettings.productName
			//var productName = PlayerSettings.productName;
            var productName = DownloadManager.SpamData.Instance.project;

            View.lbTitle.text = "Retrieving Bundles, Please Wait...";

            _api.RequestBundleVersioning(productName, cachedVersionTs: cachedTs, callback: SendVersioningResponse);
            _api.RequestPreviousCompilations(productName, cachedVersionTs: cachedTs, callback: SendPrevCompilationsResponse);
        }

        //Async
        public void SendVersioningResponse(Attr response)
        {
            versioningReceivedEvent.Send(View.window, response);
        }


        //Sync
        public void OnVersioningResponse(Attr response)
        {
            if(response.AsDic["response"].AsDic["result"].AsValue != "OK")
            {
                Debug.LogError("Error retrieving Versioning:");
                Debug.LogError(response.ToString());
                return;
            }

            if(!response.AsDic["response"].AsDic["cached"].AsValue.ToBool() ||
               !Model.InitFromCache())
            {
                Model.InitFromResponse(response);
            }

            Reload();
        }


        //Async
        public void SendPrevCompilationsResponse(Attr response)
        {
            prevCompilationsReceivedEvent.Send(View.window, response);
        }

        //Sync
        public void OnPreviousRequestsResponded(Attr response)
        {
            Model.LoadCompilationResults(response);

            newCompilationResultCreated.Send(View.window);
        }



        public override void Update( double elapsed )
        {
            _lastCompilationPollTime += (long)(elapsed * 1000);

            //Poll for task status
            if(Model.IsDataLoaded)
            {
                if(_lastCompilationPollTime > MIN_TASK_STATUS_POLL_TIME)
                {
                    _lastCompilationPollTime = 0;

                    //poll every pending compilation
                    var pendingCompilations = Model.PendingCompilations.Select(x => x.Id).ToArray();
                    if(pendingCompilations.Length > 0)
                    {
                        _api.PollCompilationStatus(pendingCompilations, callback: PollCompilationsCallback);
                    }
                }
            }
            
            base.Update(elapsed);
        }

        void OnVersionChange(TLListSelectorItem newVersion)
        {
            string version = newVersion.Content;

            var bundleDatas = Model.bdataDict[version];
            // will be null if is_develop version
            var taggedVersion = Model.brResponse.data.tagged_project_versions[version];
            var isDevVersion = taggedVersion.is_develop;

            View.twBundleSelector.SetListItems(bundleDatas, taggedVersion.versioning, isDevVersion, sorted: true);
        }
        
        void OnCheckChanged(bool value)
        {
            SPAMConfigJson.Instance.autoSplitEnabled = value;
        }


        void OnRequestBundlesButton()
        {
            var requestedBundles = View.twBundleSelector.Requested;

            if(CurrentVersion != null && CurrentVersion != string.Empty && requestedBundles.Count() > 0)
            {        
                    //Found an invalid "Dragon S.", cannot rely on PlayerSettings.productName
                    //var productName = PlayerSettings.productName;
                    var productName = DownloadManager.SpamData.Instance.project;
                areRequestsPending = _api.RequestCreateBundlesByName(requestedBundles, productName, CurrentVersion, callback: RequestedBundlesCallback);

                //disable request button
                View.btRequestBundles.SetDisabled(true);
            }
        }


        private System.Object _responseLock = new System.Object();

        void OnCompilationResultCreated()
        {
            lock (_responseLock)
            {
                //refresh the view models
                List<BRCompilationResultSelectorItem> listCompRes = new List<BRCompilationResultSelectorItem>();
                for (int i = 0; i < Model.compilationResults.Count; i++)
                {
                    BRCompilationResultSelectorItem compRes = new BRCompilationResultSelectorItem(Model.compilationResults[i], View);
                    compRes.CompilationResultClickedEvent.Connect(View.twBundleSelector.MarkRequestedItems);
                    listCompRes.Add(compRes);
                }
                View.twCompilationResult.SetListItems(listCompRes);
            }
        }

        // async request callback
        void RequestedBundlesCallback(Attr response)
        {
            //enable request button
            View.btRequestBundles.SetDisabled(false);

            try
            {
                var responseContents = response.AsDic["response"];
                var additionalInfo = response.AsDic["additional_info"];

                if(responseContents.AsDic.GetValue("result").ToString() != "OK")
                {
                    throw new Exception("Bad response");
                }

                var compilation_name = responseContents.AsDic.GetValue("compilation_name").ToString();
                var compilation_id = responseContents.AsDic.GetValue("compilation_id").ToInt();
                var bundles = additionalInfo.AsDic["bundles"].AsList.ToList<string>();
                Debug.Log(String.Format("<color=green>Successfully sent request:</color> {0}", compilation_name));

                //create a pending compilation result for polling
                var compilationResult = new BRCompilationResult(compilation_id);
                compilationResult.SetBundles(bundles);

                //string userEmail = SPAMAuthenticator.GetCachedUser();
                compilationResult.author = additionalInfo.AsDic["author"].ToString();
                lock (_responseLock)
                {
                    if (Model.AddCompilationResult(compilationResult))
                    {
                        newCompilationResultCreated.Send(View.window);
                    }
                }

                if (areRequestsPending)
                {
                    areRequestsPending = _api.MakeNextRequest();
                }                            

            }
            catch
            {
                Debug.LogError(response.ToString());
            }
        }

        // async request callback
        void PollCompilationsCallback(Attr response)
        {
            try
            {
                var responseContents = response.AsDic["response"];
                var compilations = responseContents.AsDic["compilations"].AsList;
                foreach(var compilation in compilations)
                {
                    int compilation_id = compilation.AsDic["compilation_id"].AsValue.ToInt();
                    string status = compilation.AsDic["status"].AsValue.ToString();
                    if(status != BRCompilationResult.CompilationState.PENDING.ToString())
                    {
                        BRCompilationResult.CompilationState statusValue = (BRCompilationResult.CompilationState)Enum.Parse(typeof(BRCompilationResult.CompilationState), status);
                        Model.CompleteCompilationResult(compilation_id, statusValue);
                    }
                }
            }
            catch
            {
                Debug.LogError(response.ToString());
            }
        }

        void OnLogoutButton()
        {
            auth.ResetLoginPrefs();
            _api = null;

            View.window.Close();
        }

        public override void OnUnload()
        {
            //Try to store the versioning response
            if(Model != null && Model.brResponse != null)
            {
                if(Model.brResponse.IsCached)
                {
                    Model.brResponse.Store();
                }
            }
        }
    }
}
