using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class Bundle
    {
        public string Name = "";
        public float Size = 0f;
        public bool IsLocal = false;
        public Asset Asset;

        public List<Bundle> Parents;
        public string Url;

        public BundleStatus Status;
        public List<BundleOperation> OperationQueue;
        public string Log;

        public Bundle(string name, float size, bool isLocal, Asset asset, List<Bundle> parents, string url, BundleStatus status, List<BundleOperation> operationQueue, string log)
        {
            Name = name;
            Size = size;
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
        Queued,
        Processing,
        Warning,
        Error}

    ;

    public enum BundleOperation
    {
        Create,
        Update,
        Remove,
        AddToBuild,
        RemoveFromBuild}

    ;
}