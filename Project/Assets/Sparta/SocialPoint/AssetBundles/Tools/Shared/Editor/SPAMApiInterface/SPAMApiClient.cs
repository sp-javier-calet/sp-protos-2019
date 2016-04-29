using System;
using System.Collections.Specialized;
using SocialPoint.Attributes;

namespace SocialPoint.Tool.Shared
{
    /*
     * Class for encapsulating the calls to SPAM services common to all unity projects
     */
    public class SPAMApiClient : SPAMApiInterface
	{
        /*
         * The following calls are deprecated services but could be useful to have as a template 
         */

        /*
		// Threaded
        public void RequestUnityProjects( Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/api/v1/project/?project_type=unity";

			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;

			Request (rParams);
		}

		// Threaded
		public void RequestAssetTypesForProject( int id, Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + string.Format("/unity/get_asset_types/{0}/", id);
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			
			Request (rParams);
		}

		// Threaded
		public void RequestAssetsForProject( int projectId, int typeId,  Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + string.Format("/unity/get_assets/{0}/{1}/", projectId, typeId);

			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			
			Request (rParams);
		}

		// Threaded
		public void RequestComponentsFoAsset( int assetId,  Action<Attr> callback=null, Attr additionalInfo=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + string.Format("/unity/get_lock_info/{0}/", assetId);
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			rParams.additionalInfo = additionalInfo;
			
			Request (rParams);
		}

		// Threaded
		public void RequestComponentsForAllAssetsOfType( int typeId,  Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + string.Format("/unity/get_lock_info_by_type/{0}/", typeId);
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			
			Request (rParams);
		}

		// Threaded
		public void RequestComponentLock( int assetId, string[] componentNames, Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity/lock_components/";

			NameValueCollection postData = new NameValueCollection ();
			postData.Add ("asset_id", assetId.ToString ());
			postData.Add ("components[]", string.Join(",", componentNames));

			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			rParams.postData = postData;
			
			Request (rParams);
		}

		// Threaded
		public void RequestComponentUnlock( int assetId, string[] componentNames, Action<Attr> callback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity/unlock_components/";
			
			NameValueCollection postData = new NameValueCollection ();
			postData.Add ("asset_id", assetId.ToString ());
			postData.Add ("components[]", string.Join(",", componentNames));
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = callback;
			rParams.postData = postData;
			
			Request (rParams);
		}

		// Threaded
		public void RequestUpdateAsset( int assetId, Action<Attr> successCallback=null, Action<float> progressCallback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + string.Format("/unity/update/{0}/", assetId);
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = successCallback;
			rParams.dnlProgress = progressCallback;
			
			Request (rParams);
		}

		// Threaded
		public void RequestCompareWithLatest( int assetId, string file, Action<Attr> successCallback=null, Action<float> progressCallback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity/compare/";

			NameValueCollection postData = new NameValueCollection ();
			postData.Add ("asset_id", assetId.ToString ());
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.uploadFile = file;
			rParams.completed = successCallback;
			rParams.uplProgress = progressCallback;
			rParams.postData = postData;
			
			Request (rParams);
		}

		//Threaded
		public void RequestCommitId( int assetId, string[] componentNames, string file, Action<Attr> successCallback=null, Action<float> progressCallback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity/request_commit/";

			NameValueCollection postData = new NameValueCollection ();
			postData.Add ("asset_id", assetId.ToString ());
			postData.Add ("components", string.Join(":", componentNames)); // Is a list, not an array

			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.uploadFile = file;
			rParams.completed = successCallback;
			rParams.uplProgress = progressCallback;
			rParams.postData = postData;
			
			Request (rParams);
		}

		//Threaded
		public void RequestConfirmCommitWithId( int commitId, Action<Attr> successCallback=null )
		{
			string url = SPAMAuthenticator.SPAM_SERVICES_ENDPOINT + "/unity/confirm_commit/";
			
			NameValueCollection postData = new NameValueCollection ();
			postData.Add ("commit_id", commitId.ToString ());
			
			RequestParams rParams = new RequestParams ();
			rParams.url = url;
			rParams.completed = successCallback;
			rParams.postData = postData;
			
			Request (rParams);
		}
        */
	}
}

