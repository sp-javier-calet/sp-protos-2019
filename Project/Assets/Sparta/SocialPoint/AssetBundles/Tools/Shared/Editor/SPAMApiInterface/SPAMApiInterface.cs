using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using SocialPoint.Attributes;

namespace SocialPoint.Tool.Shared
{
	public sealed class RequestParams
	{
		public NameValueCollection postData;
		public string contentType;
		public string url;
		public string uploadFile;
		public Attr additionalInfo;
		public Action<Attr> completed;
		public Action<float> dnlProgress;
		public Action<float> uplProgress;
	}
	
	public sealed class RequestResults
	{
		public string responseContent;
		public string tmpDownloadPath;
        public Attr additionalInfo;
		public Action<Attr> completed;
	}

	public sealed class RequestProgressParams
	{
		public Action<float> progress;
	}

    public partial class SPAMApiInterface
	{
		private	SPAMAuthenticator 		_auth;
		private List<BackgroundWorker> 	_requestWorkers;
		
		public SPAMApiInterface ()
		{
			_auth = new SPAMAuthenticator ();
			_requestWorkers = new List<BackgroundWorker> ();
		}
		
		private void SetupWorker(BackgroundWorker worker)
		{
			worker.WorkerSupportsCancellation = false;
			worker.DoWork += new DoWorkEventHandler (RequestWork);
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler (RequestCompleted);
		}

		// Request for JSON objects
		protected void Request( RequestParams rParams )
		{
			BackgroundWorker worker = null;
            for (int i = 0; i < _requestWorkers.Count; ++i)
            {
                worker = !_requestWorkers[i].IsBusy ? _requestWorkers[i] : null;
                if (worker != null)
                    break;
            }
			
			if (worker == null) {
				worker = new BackgroundWorker();
				SetupWorker(worker);
				_requestWorkers.Add(worker);
			}

			worker.RunWorkerAsync (rParams);
		}
		
		void RequestWork(object sender, DoWorkEventArgs e)
		{
			try
			{
                RequestParams rParams = (RequestParams)e.Argument;
				RequestResults rResults = new RequestResults ();

				HttpWebResponse response;              



                // Request upload?
                if (rParams.uploadFile != null && rParams.uploadFile != "")
					response = _auth.PerformAuthenticatedRequestForUploading(rParams.url,
					                                                         rParams.uploadFile,
					                                                         rParams.postData,
					                                                         rParams.uplProgress);                                                                    
				else
					response = _auth.PerformAuthenticatedRequest(rParams.url, rParams.postData);

                using (response)
                {

                    // download treatment if attachment is found in the response
                    if (response != null && RequiresDownload(response))
                    {
                        string tempDownloadPath = Path.Combine(Path.GetTempPath(), GetAttachmentFileName(response));

                        RequestProgressParams rpParams = null;
                        if (rParams.dnlProgress != null)
                        {
                            rpParams = new RequestProgressParams();
                            rpParams.progress = rParams.dnlProgress;
                        }

                        DownloadAttachment(response, tempDownloadPath, rpParams);
                        rResults.tmpDownloadPath = tempDownloadPath;
                    }


                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    { 
                        rResults.responseContent = reader.ReadToEnd(); 
                    }
                    response.Close(); 
                    
                    rResults.completed = rParams.completed;
                    rResults.additionalInfo = rParams.additionalInfo;


                    e.Result = rResults;
                }
			}
			catch (Exception exc)
			{
				Debug.LogError (exc);
			}
		}
		
		void RequestCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			string responseContent;
			RequestResults rResults;

			if (e.Error != null) {
				Debug.LogError (e.Error.Message);
				responseContent = "Error on request BackgroundWorker";
				rResults = new RequestResults();

			} else {
				rResults = (RequestResults)e.Result;
                //if (rResults.response == null){
                //	responseContent = "Error on request server response";
                //}
                //else {
                //                using(StreamReader reader= new StreamReader(rResults.response.GetResponseStream()))
                //                {
                //	    responseContent = reader.ReadToEnd ();
                //                }
                //}
                responseContent = rResults.responseContent;
			}

			try {
                AttrDic result = new AttrDic();

				if (e.Error != null)
                    result["error"] = new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(responseContent));
				else {
					if (responseContent.Length <= 0)
						result["response"] = new AttrDic();
					else
                        result["response"] = new JsonAttrParser().Parse(System.Text.ASCIIEncoding.ASCII.GetBytes(responseContent));

					if (rResults.tmpDownloadPath != null)
						result["downloaded_file"] = new AttrString(rResults.tmpDownloadPath);
					
					if (rResults.additionalInfo != null)
                        result["additional_info"] = rResults.additionalInfo;
				}

				if (rResults.completed != null)
					rResults.completed(result);

			} catch (Exception exc){
                AttrDic result = new AttrDic();

				string errString = string.Format("Exception parsing response\nException:\n{0}\n.\nResponse Content:\n{1}", exc, responseContent);
                result["error"] = new AttrString(errString);

				if (rResults.completed != null)
					rResults.completed(result);
			}
		}
	}
}


