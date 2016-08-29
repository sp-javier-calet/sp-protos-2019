using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SocialPoint.Tool.Shared;
using SocialPoint.Attributes;
using LitJson;
using System.Linq;
using UnityEngine;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class ReqBundleData
    {
        public string name;
        public int version;
        public bool is_included;

        public ReqBundleData(BundleSelectorItem item)
        {
            name = item.Content;
            version = 1; // TODO: version is not used anymore, fake this one
            is_included = item.Included;
        }

        public override string ToString()
        {
            return JsonMapper.ToJson(this);
        }
    }

    public sealed class SplittedBundleRequest
    {
        public List<BundleSelectorItem> bundles;
        public string projectName;
        public string projectVersion;
        public Action<Attr> callback;

        public SplittedBundleRequest(string projectName, string projectVersion, Action<Attr> callback)
        {
            bundles = new List<BundleSelectorItem>();
            this.projectName = projectName;
            this.projectVersion = projectVersion;
            this.callback = callback;
        }

    }

    /// <summary>
    /// Encapsulates the SPAM calls for the Bundle Request client aplication
    /// </summary>
    public sealed class SPAMApiBundleRequest : SPAMApiInterface
    {
        static readonly Action<Attr> LogCallback = delegate(Attr response) {
            Debug.Log("-- LogCallback --");
            Debug.Log(response.ToString());
        };



        public List<SplittedBundleRequest> pendingRequests = new List<SplittedBundleRequest>();

        // Threaded
        public bool RequestCreateBundlesByName(IEnumerable<BundleSelectorItem> selectedBundles, string projectName, string projectVersion, Action<Attr> callback=null)
        {
            callback = callback ?? LogCallback;

            List<int> count = new List<int>();
            List<HashSet<BundleSelectorItem>> includedParents = new List<HashSet<BundleSelectorItem>>();
            pendingRequests.Add(new SplittedBundleRequest(projectName,projectVersion, callback));
            count.Add(0);
            includedParents.Add(new HashSet<BundleSelectorItem>());
            int requestNumber = 0;


            if (!SPAMConfigJson.Instance.autoSplitEnabled)
            {
                pendingRequests[0].bundles.AddRange(selectedBundles);
            }
            else { 
                //Splitting
                foreach (var b in selectedBundles)
                {
                    if (requestNumber >= pendingRequests.Count)
                    {
                        includedParents.Add(new HashSet<BundleSelectorItem>());
                        pendingRequests.Add(new SplittedBundleRequest(projectName, projectVersion, callback));
                        count.Add(0);
                    }

                    int idx = requestNumber;
                    int finalCount = -1;
                    int highestCount = -1;

                    for (int i = 0; i <= requestNumber; i++)
                    {
                        int countForThisRequest = GetBundleDependantsCount(b, includedParents[i]);

                        finalCount = countForThisRequest;

                        if(countForThisRequest == 0)
                        {
                            idx = i;
                            break;
                        }else if(count[i] + countForThisRequest <= SPAMConfigJson.Instance.maxBundlesPerRequest && count[i]+countForThisRequest > highestCount)
                        {
                            highestCount = count[i] + countForThisRequest;
                            idx = i;
                        }
                    }

                    pendingRequests[idx].bundles.Add(b);
                    includedParents[idx].Add(GetFirstParent(b));
                    count[idx] += finalCount;


                    if (count[requestNumber] >= SPAMConfigJson.Instance.maxBundlesPerRequest && finalCount != 0)
                    {
                        if (count[requestNumber] > SPAMConfigJson.Instance.maxBundlesPerRequest && pendingRequests[requestNumber].bundles.Count > 1)
                        {
                            pendingRequests[requestNumber].bundles.Remove(b);
                            includedParents[requestNumber].Remove(GetFirstParent(b));
                            requestNumber++;
                            pendingRequests.Add(new SplittedBundleRequest(projectName, projectVersion, callback));
                            includedParents.Add(new HashSet<BundleSelectorItem>());
                            pendingRequests[requestNumber].bundles.Add(b);

                            count[idx] -= finalCount;

                            finalCount = GetBundleDependantsCount(b, includedParents[requestNumber]);
                            includedParents[requestNumber].Add(GetFirstParent(b));
                            count.Add(finalCount);

                        }
                        else
                        {
                            requestNumber++;
                        }
                    }
                }

                //clean empty requests (should only be the last one)
                for (int i = 0; i < pendingRequests.Count; i++)
                {
                    if (pendingRequests[i].bundles.Count == 0)
                    {
                        pendingRequests.RemoveAt(i);
                        i--;
                    }
                }
            }


            //Make first request
            return MakeNextRequest();
        }

        /// <summary>
        /// Sends the first bundle creation requests in the queue and removes it
        /// </summary>
        /// <returns>wether or not there are more pending requests</returns>
        public bool MakeNextRequest()
        {
            if (pendingRequests != null && pendingRequests.Count > 0) {

                SplittedBundleRequest request = pendingRequests[0];
                pendingRequests.RemoveAt(0);

                string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_export_bundle_set_post/";

                RequestParams rParams = new RequestParams();
                rParams.url = url;
                rParams.completed = request.callback;
                rParams.postData = new NameValueCollection();

                var reqBundleData = new List<ReqBundleData>();
                foreach (var b in request.bundles)
                {
                    reqBundleData.Add(new ReqBundleData(b));
                }

                rParams.postData["bundles"] = GetEncodedValues((IEnumerable)reqBundleData);
                rParams.postData["project_name"] = request.projectName;
                rParams.postData["project_version"] = request.projectVersion;

                //This param will come in the response
                rParams.additionalInfo = new AttrDic();
                var bundleList = new AttrList(request.bundles.Select(x => (Attr)new AttrString(x.Content)).ToList());
                rParams.additionalInfo.AsDic["bundles"] = bundleList;

                string userEmail = SPAMAuthenticator.CachedUser;
                rParams.additionalInfo.AsDic["author"] = new AttrString(userEmail.Substring(0, userEmail.IndexOf('@')));

                Request(rParams);
            } else
                Debug.LogError("No requests pending");


            return pendingRequests.Count > 0;
        }


        BundleSelectorItem GetFirstParent(BundleSelectorItem bundle)
        {
            while (bundle.GetParent() != null)
            {
                bundle = bundle.GetParent();
            }

            return bundle;
        }


        int GetBundleDependantsCount(BundleSelectorItem bundle, HashSet<BundleSelectorItem> includedParents)
        {
            List<BundleSelectorItem> res = new List<BundleSelectorItem>();
            BundleSelectorItem parentBundle = GetFirstParent(bundle);


            if (!includedParents.Contains(parentBundle))
            {
                res = parentBundle.GetChilds(false);
                res.Add(parentBundle);
            }

            return res.Count;
        }


        // Threaded
        public void RequestBundleVersioning(string projectName, int cachedVersionTs=-1, Action<Attr> callback=null)
        {
            callback = callback ?? LogCallback;

            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_get_versioning_for_active_project_versions/";

            RequestParams rParams = new RequestParams ();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["project_name"] = projectName;
            //timestamp of the last cached version, this can prevent uneeded requests. If provided, enabled caching
            if(cachedVersionTs != -1)
            {
                rParams.postData["cached_version"] = cachedVersionTs.ToString();
            }

            Request (rParams);
            Debug.Log("versioning sent");
        }

        // Threaded
        public void RequestPreviousCompilations(string projectName, int cachedVersionTs = -1, Action<Attr> callback = null)
        {
            callback = callback ?? LogCallback;

            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_get_last_finished_compilations/";

            RequestParams rParams = new RequestParams();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["project_name"] = projectName;
            rParams.postData["limit"] = SPAMConfigJson.Instance.pastCompilationsShown.ToString();

            Request(rParams);
        }



        //Threaded
        public void PollCompilationStatus( int[] compilationIds, Action<Attr> callback=null )
        {
            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_get_compilation_process_status/";
            
            RequestParams rParams = new RequestParams ();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["compilation_ids"] = GetEncodedValues((IEnumerable)compilationIds);
            
            Request (rParams);
        }

        string GetEncodedValues(IEnumerable values)
        {
            StringBuilder encodedValues = new StringBuilder("[");
            foreach(object value in values)
            {
                encodedValues.Append(value.ToString());
                encodedValues.Append(',');
            }

            if(encodedValues[encodedValues.Length - 1] == ',')
            {
                encodedValues[encodedValues.Length - 1] = ']';
            }
            else
            {
                encodedValues.Append(']');
            }

            return encodedValues.ToString();
        }
    }
}

