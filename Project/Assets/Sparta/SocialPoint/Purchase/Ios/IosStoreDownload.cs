using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE
namespace SocialPoint.Purchase
{
    public enum IosStoreDownloadState
    {
        Waiting,
        Active,
        Paused,
        Finished,
        Failed,
        Cancelled
    }


    public class IosStoreDownload
    {
        IosStoreDownloadState _downloadState;
        double _contentLength;
        string _contentIdentifier;
        string _contentURL;
        string _contentVersion;
        string _error;
        float _progress;
        double _timeRemaining;
        IosStoreTransaction _transaction;


        public static List<IosStoreDownload> DownloadsFromJson(string json)
        {
            var downloadList = new List<IosStoreDownload>();

            //UPDATE NEEDED!
            List<object> downloads = null;//json.listFromJson();
            if(downloads == null)
                return downloadList;

            foreach(Dictionary<string, object> dict in downloads)
                downloadList.Add(DownloadFromDictionary(dict));

            return downloadList;
        }


        public static IosStoreDownload DownloadFromDictionary(Dictionary<string,object> dict)
        {
            var download = new IosStoreDownload();

            if(dict.ContainsKey("downloadState"))
                download._downloadState = (IosStoreDownloadState)int.Parse(dict["downloadState"].ToString());

            if(dict.ContainsKey("contentLength"))
                download._contentLength = double.Parse(dict["contentLength"].ToString());

            if(dict.ContainsKey("contentIdentifier"))
                download._contentIdentifier = dict["contentIdentifier"].ToString();

            if(dict.ContainsKey("contentURL"))
                download._contentURL = dict["contentURL"].ToString();

            if(dict.ContainsKey("contentVersion"))
                download._contentVersion = dict["contentVersion"].ToString();

            if(dict.ContainsKey("error"))
                download._error = dict["error"].ToString();

            if(dict.ContainsKey("progress"))
                download._progress = float.Parse(dict["progress"].ToString());

            if(dict.ContainsKey("timeRemaining"))
                download._timeRemaining = double.Parse(dict["timeRemaining"].ToString());

            if(dict.ContainsKey("transaction"))
                download._transaction = IosStoreTransaction.TransactionFromDictionary(dict["transaction"] as Dictionary<string,object>);

            return download;
        }


        public override string ToString()
        {
            return String.Format("<IosStoreDownload> downloadState: {0}\n contentLength: {1}\n contentIdentifier: {2}\n contentURL: {3}\n contentVersion: {4}\n error: {5}\n progress: {6}\n timeRemaining: {7}\n transaction: {8}",
                _downloadState, _contentLength, _contentIdentifier, _contentURL, _contentVersion, _error, _progress, _timeRemaining, _transaction);
        }
    }
}
#endif
