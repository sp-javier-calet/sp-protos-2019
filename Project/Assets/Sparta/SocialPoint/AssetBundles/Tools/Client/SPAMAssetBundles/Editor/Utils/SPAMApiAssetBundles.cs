using System;
using System.IO;
using System.Collections.Specialized;
using SocialPoint.Tool.Shared;
using SocialPoint.Attributes;

namespace SocialPoint.Editor.SPAMGui
{
    /*
     * Class for encapsulating the calls to SPAM services related to asset bundle creation
     */
    public sealed class SPAMApiAssetBundles : SPAMApiInterface
    {
        // Threaded
        public void RequestBundleCreation( BundleMetaData bundleToExport, string projectName, Action<Attr> callback=null )
        {
            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_export_bundle_scene_post/";

            RequestParams rParams = new RequestParams ();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["pack_name"] = bundleToExport.packName;
            rParams.postData["project_name"] = projectName;
            rParams.postData["bundle_name"] = bundleToExport.name;
            rParams.postData["version"] = bundleToExport.version.ToString();
            rParams.postData["is_builtin"] = bundleToExport.isBundled.ToString();

            //This param will come in the response
            rParams.additionalInfo = new AttrDic();
            rParams.additionalInfo.AsDic["scene_name"] = bundleToExport.mainAssetPath != string.Empty ? new AttrString(bundleToExport.mainAssetPath) : new AttrString(bundleToExport.name);
            rParams.additionalInfo.AsDic["bundle_name"] = new AttrString(bundleToExport.name);
            rParams.additionalInfo.AsDic["version"] = new AttrString(bundleToExport.version.ToString());
            
            Request (rParams);
        }

        // Threaded
        public void PollTaskStatus( string taskId, Action<Attr> callback=null )
        {
            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/task_json_post/";
            
            RequestParams rParams = new RequestParams ();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["task_id"] = taskId;

            //This param will come in the response
            rParams.additionalInfo = new AttrDic();
            rParams.additionalInfo.AsDic["task_id"] = new AttrString(taskId);

            Request (rParams);
        }

        // Threaded
        public void GetTaskExportBundleSize( string taskId, Action<Attr> callback=null )
        {
            string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity_check_exported_bundle_size_post/";

            RequestParams rParams = new RequestParams ();
            rParams.url = url;
            rParams.completed = callback;
            rParams.postData = new NameValueCollection();
            rParams.postData["task_id"] = taskId;

            //This param will come in the response
            rParams.additionalInfo = new AttrDic();
            rParams.additionalInfo.AsDic["task_id"] = new AttrString(taskId);
            
            Request (rParams);
        }
    }
}