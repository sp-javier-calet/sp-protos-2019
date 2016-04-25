using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadsQueue : MonoBehaviour
{
    static DownloadsQueue instance = null;
	public Action<string> CallbackByAsset = null;
    bool _continueDownload = true;

    int _assetCounter;
    /**
     * Get instance of DownloadQueue.
     * This prop will create a GameObject named Download Manager in scene when first time called.
     */ 
    public static DownloadsQueue Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("DownloadsQueue").AddComponent<DownloadsQueue>();
                
                DontDestroyOnLoad(instance.gameObject);
            }
            
            return instance;
        }
    }

	public void StartDownloadQueue(List<string> bundlePaths, Action callback, Action<string> callbackByAsset = null)
    {
        Debug.Log("---------------- StartDownloadQueue --------------");
		StartCoroutine(StartDownloadQueueLoop(bundlePaths, callback, callbackByAsset));
    }

	public IEnumerator StartDownloadQueueLoop(List<string> bundlePaths, Action callback, Action<string> callbackByAsset = null)
    {
		CallbackByAsset = callbackByAsset;
        _assetCounter = 0;
        _continueDownload = true;

        for (int i=bundlePaths.Count-1; i>=0; --i)
        {
            if( DownloadManager.Instance.GetWWW(bundlePaths[i]) != null ) 
            {
                bundlePaths.RemoveAt(i);
            }
        }

        _assetCounter = bundlePaths.Count;
            
        for (int i=0; i<bundlePaths.Count; i++)
        {
            DownloadManager.Instance.StartDownload(bundlePaths[i], -1, ErrorCallback);
        }

        while(_continueDownload)
		{
            if( _assetCounter <= 0 )
            {
    	        if (callback != null) 
    	        {
                    Debug.Log("**************** Download Queue Calback **********************");
    	            callback();
    	        }
                _continueDownload = false;
            }

            yield return null;
		}
    }

	void ErrorCallback(string msg)
	{
        if(msg.Contains("Error") )
        {
            _continueDownload = false;
            if( CallbackByAsset != null )
            {
                CallbackByAsset(msg);
            }
        }
        else
        {
            --_assetCounter;
        }
	}
}
