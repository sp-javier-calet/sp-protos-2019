﻿using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class Bundle
    {
        public string Name = "";
        public Dictionary<BundlePlaform, int> Size;
        public bool IsAutogenerated = false;
        public bool IsLocal = false;
        public Asset Asset;

        public List<Bundle> Parents;
        public Dictionary<BundlePlaform, string> Url;

        public BundleStatus Status;
        public Dictionary<int, BundleOperation> OperationQueue;
        public string Log;

        public Bundle(string name, Dictionary<BundlePlaform, int> size, bool isAutogenerated, bool isLocal, Asset asset, List<Bundle> parents, Dictionary<BundlePlaform, string> url, BundleStatus status, Dictionary<int, BundleOperation> operationQueue, string log)
        {
            Name = name;
            Size = size;
            IsAutogenerated = isAutogenerated;
            IsLocal = isLocal;
            Asset = asset;
            Parents = parents;
            Url = url;
            Status = status;
            OperationQueue = operationQueue;
            Log = log;
        }
    }

    public enum BundleStatus
    {
        Deployed,
        Processing,
        Queued,
        Warning,
        Error
    }

    public enum BundleOperation
    {
        create_asset_bundles,
        Remove,
        AddToBuild,
        RemoveFromBuild
    }

    public enum BundlePlaform
    {
        ios,
        android_etc
    }
}