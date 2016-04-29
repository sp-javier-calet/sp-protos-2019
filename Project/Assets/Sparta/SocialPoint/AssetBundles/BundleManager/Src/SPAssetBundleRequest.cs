using System;
using System.Collections;
using UnityEngine;

public class SPAssetBundleRequest {

    public UnityEngine.Object asset;
    public AssetBundle bundle;
    public string name;
    public Type type;
    public Exception exception;

    public Coroutine Start()
    {
        return DownloadManager.Instance.StartCoroutine(DoAssetLoading());
    }
    
    private IEnumerator DoAssetLoading()
    {
    
        AssetBundleRequest request = bundle.LoadAssetAsync (name, type);
        while(!request.isDone)
        {
            yield return null;
        }
            
        GameObject gameObject = request.asset as GameObject;
        try {
            DownloadManager.LinkBehaviours(gameObject, name, bundle);
        }
        catch (Exception e) {
            this.exception = e;
        }
        this.asset = gameObject;
        
    }
    
}
