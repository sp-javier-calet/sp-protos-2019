using UnityEngine;
using System;

public static class AssetBundleExtensions {

    public static SPAssetBundleRequest LoadAsyncAndLinkBehaviours(this AssetBundle bundle, string name, Type type) {
        
        SPAssetBundleRequest request = new SPAssetBundleRequest();
        
        request.bundle = bundle;
        request.name = name;
        request.type = type;
        
        return request;
        
    }
}
